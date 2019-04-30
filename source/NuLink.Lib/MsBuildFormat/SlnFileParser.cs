using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NuLink.Lib.MsBuildFormat
{
    public class SlnFileParser
    {
        private static readonly Regex CrackProjectLine = new Regex(
            "^"
            + "Project\\(\"(?<PROJECTTYPEGUID>.*)\"\\)"
            + "\\s*=\\s*"
            + "\"(?<PROJECTNAME>.*)\""
            + "\\s*,\\s*"
            + "\"(?<RELATIVEPATH>.*)\""
            + "\\s*,\\s*"
            + "\"(?<PROJECTGUID>.*)\""
            + "$"
        );

        private static readonly Regex CrackGlobalSectionLine = new Regex(
            "^"
            + "\\s*"
            + "GlobalSection\\((?<SECTIONNAME>.*)\\)"
            + "\\s*=\\s*"
            + "(?<SECTIONKIND>.*)"
            + "$"
        );

        private static readonly Regex CrackPropertyLine = new Regex(
            "^"
            + "\\s*"
            + "(?<PROPERTYNAME>[^=]*)"
            + "\\s*=\\s*"
            + "(?<PROPERTYVALUE>[^=]*)"
            + "$"
        );

        private readonly TextReader _reader;

        public SlnFileParser(TextReader reader)
        {
            _reader = reader;
        }

        public void Parse(SlnFile sln)
        {
            int lineNumber = 0;

            ParseLines(
                untilLine: null,
                regex: new[] {CrackProjectLine, CrackPropertyLine, new Regex("^Global$"), new Regex("^.*$")},
                onMatch: (regexIndex, match) => {
                    switch (regexIndex)
                    {
                        case 0:
                            ParseProject(match);
                            break;
                        case 1:
                            sln.Parts.Add(PropertySlnFilePart.Parse(match));
                            break;
                        case 2:
                            ParseGlobalSections();
                            break;
                        case 3:
                            sln.Parts.Add(TextLineSlnFilePart.Parse(match));
                            break;
                    }
                });

            void ParseProject(Match match)
            {
                sln.Parts.Add(ProjectSlnFilePart.Parse(match));
                ParseLines(untilLine: "EndProject", regex: null, onMatch: null);
            }

            void ParseGlobalSections()
            {
                var globalPart = new GlobalSlnFilePart();
                sln.Parts.Add(globalPart);

                ParseLines(
                    untilLine: "EndGlobal",
                    regex: new[] {CrackGlobalSectionLine},
                    onMatch: (regexIndex, match) => {
                        var section = SlnSection.Parse(match);
                        ParseGlobalSectionProperties(section);
                        globalPart.Sections.Add(section);
                    });
            }

            void ParseGlobalSectionProperties(SlnSection section)
            {
                ParseLines(
                    untilLine: "\tEndGlobalSection",
                    regex: new[] {CrackPropertyLine},
                    onMatch: (regexIndex, match) => { section.Properties.Add(PropertySlnFilePart.Parse(match)); });
            }


            void ParseLines(string untilLine, Regex[] regex, Action<int, Match> onMatch)
            {
                string line;

                while ((line = _reader.ReadLine()) != null)
                {
                    lineNumber++;

                    if (line == untilLine)
                    {
                        return;
                    }

                    if (regex == null)
                    {
                        continue;
                    }

                    var matchedAny = false;

                    for (int i = 0; i < regex.Length; i++)
                    {
                        var match = regex[i].Match(line);
                        if (match.Success)
                        {
                            matchedAny = true;
                            onMatch(i, match);
                            break;
                        }
                    }

                    if (!matchedAny)
                    {
                        throw new CommandException(FormatParserError(
                            "unexpected format of global section property"));
                    }
                }

                if (untilLine != null)
                {
                    throw new CommandException(FormatParserError($"expected '{untilLine}'"));
                }
            }

            string FormatParserError(string message)
            {
                return $"{sln.FileInfo.FullName}({lineNumber}): parser error: {message}";
            }
        }
    }
}

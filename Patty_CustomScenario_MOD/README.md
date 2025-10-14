# Custom Scenario
A mod that allows you to make any custom scenario just by editing `.json` files. Doesn't support Ascension mode yet

The mod will extract the scenario from the game, then convert it into a human readable format, and load that file back into the game.

There's a bunch of configuration you can setup to change the mod functionality
```
[CustomScenario]
# When enabled, always start a scenario with debug data
Debug = false
# When enabled, replace debug scenario with file specified on Debug folder
ReplaceDebugScenario = true
# When enabled, replace endless scenario with file specified on Endless folder
ReplaceEndlessScenario = true
# When enabled, replace normal mode scenario with file specified on Ascension folder
ReplaceNormalModeScenario = true
# When enabled, replace roguelike scenario with file specified on Roguelike folder
ReplaceRoguelikeScenario = true
# When enabled, replace roguelike standard scenario with file specified on RoguelikeStandard
ReplaceRoguelikeStandardScenario = true
# When enabled, extract scenario data from the game if the file doesn't exist yet
ExtractOriginalFiles = true
# When enabled, load custom scenario data from Endless folder
AddCustomEndlessScenario = true
# When enabled, load scenario data that's not registered in Endless folder
# May added the game scenario as well. Only enable if you know what you're doing
LoadUnregisteredScenarioData = false
```
You can find this settings at `UserData/CustomScenarioSettings.cfg`. Run the game at least onces to see the settings. All eligible character are extracted into `AllCharacterName.txt` you can find next to the mod DLL.

Note:
You can only have 5 character minimum and 11 maximum, and more or less than that will break the game for now.

Currently available character (May support modded role, but i don't promise if it'll work):
```
Villager:
- Alchemist
- Architect
- Baker
- Bard
- Bishop
- Confessor
- Dreamer
- Druid
- Empress
- Enlightened
- Fortune Teller
- Gemcrafter
- Hunter
- Jester
- Judge
- Knight
- Knitter
- Lover
- Medium
- Oracle
- Poet
- Scout
- Slayer
- Saint
- Bounty Hunter
- Witness

Outcast:
- Wretch
- Bombardier
- Plague Doctor
- Drunk
- Doppelganger

Minion:
- Witch
- Twin Minion
- Shaman
- Poisoner
- Minion
- Counsellor
- Puppeteer
- Puppet
- Wretch
- Marionette

Demon:
- Pooka
- Mutant
- Baa
- Lilis

Unspecified:
```

# Copilot Instructions

## Project Guidelines
- Helper executable should remain lightweight and avoid active checking/polling; it should wait passively until the game process exits, then restore resolution.
- In ResolutionSwitcher, profile switching must save the outgoing profile's UI state before loading the incoming profile. Reset/master-reset operations should go through ConfigManager helper methods (not raw config.json file deletion) so the live in-memory config and UI can refresh immediately without requiring an app restart.
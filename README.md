# Unity Roguelike

Unity 6000.4.7f1 project following Unity Learn's 2D Roguelike tutorial.

## Unit Tests

EditMode tests live in `Assets/Tests/EditMode`.

Run them locally from a terminal with Unity installed:

```powershell
.\scripts\run-editmode-tests.ps1
```

Set `UNITY_EDITOR_PATH` first if Unity is installed somewhere other than `C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe`.

The current tests cover `BoardManager` board generation and cell lookup, `PlayerController` movement and blocked movement, and verify that `Assets/Scenes/Main.unity` assigns `Scavengers2_SpriteSheet_39` as the ground tile and `Scavengers2_SpriteSheet_1` through `Scavengers2_SpriteSheet_8` as wall tiles.

The script enables Unity Code Coverage for `Assembly-CSharp` and fails if reported line coverage is below 90%.

## GitHub Actions

The `CI` workflow runs on pull requests and pushes to `main`.

### Unit Tests

`Unit Tests / Unity EditMode` is present but disabled by default with the repository variable `UNITY_CI_ENABLED`.

To enable Unity tests in GitHub Actions, add the required Unity activation secrets (`UNITY_LICENSE`, and when needed `UNITY_EMAIL` and `UNITY_PASSWORD`) and set repository variable `UNITY_CI_ENABLED` to `true`.

### Code Scanning: Quality

`Code Scanning / Quality` runs `actionlint` against GitHub Actions workflow files.

### Code Scanning: Security

`Code Scanning / Security` runs CodeQL for C# and GitHub Actions using `build-mode: none`, so it does not need Unity installed to analyze the repository.

### Dependency Automation

Dependabot is configured for GitHub Actions updates in `.github/dependabot.yml`.

Dependency Review runs on pull requests and fails when a dependency change introduces a high severity vulnerability.

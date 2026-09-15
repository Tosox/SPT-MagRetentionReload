# Mag Retention Reload

[![Total Downloads](https://img.shields.io/github/downloads/Tosox/SPT-MagRetentionReload/total.svg?label=Downloads%20(All%20Time))](https://github.com/Tosox/SPT-MagRetentionReload/releases) [![Latest Release Downloads](https://img.shields.io/github/downloads/Tosox/SPT-MagRetentionReload/latest/total.svg?label=Downloads%20(Latest%20Release))](https://github.com/Tosox/SPT-MagRetentionReload/releases/latest)

> **UI Fixes** by **Tyfon** has a mag retention feature as well. This mod keeps it simple and adds no reload time penalty.
> If you run both, this one takes over. Unticking `Enabled` hands the feature straight back to **UI Fixes**.

## 📜 Description

**Mag Retention Reload** is a mod for *Single Player Tarkov* that adds proper magazine retention to weapon reloads by automatically stowing the old magazine back into the rig if possible instead of dropping it.

## ✨ Features

* Swaps the old magazine into the slot the new magazine was taken from
* Prevents unnecessary magazine drops during normal reloads if the magazine fits in the rig
* Works with both the reload hotkey and the inventory context menu
* Notifies you when you lose a magazine during reload
* Optionally ties the feature to maxed-out weapon mastering

## 📁 Installation

* Download the latest release
* Copy the `BepInEx` folder into your SPT folder
* Start the game

## ⚙️ Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Untick to stop retaining your magazines |
| `Require Max Weapon Mastering` | `false` | Only retain magazines once the weapon's mastering is maxed-out |
| `Notify on Dropped Magazine` | `true` | Display a notification when a magazine is dropped during a reload |

## 🤝 Fika

When playing with **Fika**, every client in the raid, **including the headless client**, should run the same version of this mod, otherwise you will run into syncing issues.
Hosts can enforce this by adding the plugin GUID to `client.mods.required` in the Fika server config, so mismatched clients are rejected when joining instead of silently desyncing:

```jsonc
"required": [ "de.tosox.magretentionreload" ]
```

## 📝 Changelog

You can check out the latest changes in [`CHANGELOG.md`](CHANGELOG.md).

## 📷 Preview

<img src="readme-res/reload.gif" alt="reload" width="500"/>

## 🙏 Acknowledgements

* Thanks to [**Lacyway**](https://github.com/Lacyway) for putting up with all my questions
* Thanks to [**Tyfon**](https://github.com/tyfon7) for the Fika sync code that inspired this one

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

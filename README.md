# Mag Retention Reload

[![Total Downloads](https://img.shields.io/github/downloads/Tosox/SPT-MagRetentionReload/total.svg?label=Downloads%20(All%20Time))](https://github.com/Tosox/SPT-MagRetentionReload/releases) [![Latest Release Downloads](https://img.shields.io/github/downloads/Tosox/SPT-MagRetentionReload/latest/total.svg?label=Downloads%20(Latest%20Release))](https://github.com/Tosox/SPT-MagRetentionReload/releases/latest)

> A similar mag retention feature is already part of **UI Fixes** by **Tyfon**.  
> I originally started working on this mod without knowing that it existed there and by the time I found out, my version was already mostly finished.
>
> This mod keeps the behavior simple and does not add any reload time penalties which may be preferable depending on your setup.
>
> If you run both mods, it is recommended to turn off **Reload Magazines In-Place** in UI Fixes.

## 📜 Description

**Mag Retention Reload** is a mod for *Single Player Tarkov* that adds proper magazine retention to weapon reloads by automatically stowing the old magazine back into the rig if possible instead of dropping it.

## ✨ Features

* Swaps the old magazine into the slot the new magazine was taken from
* Prevents unnecessary magazine drops during normal reloads if the magazine fits in the rig
* Works with both the reload hotkey and the inventory context menu
* Ties the feature to maxed out weapon mastering

## 📁 Installation

* Download the latest release
* Copy the `BepInEx` folder into your SPT folder
* Start the game

## ⚙️ Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Untick to disable magazine retention |
| `Require Max Weapon Mastering` | `true` | Only retain magazines once the weapon's mastering is maxed out |

## 🤝 Fika

When playing with **Fika**, every client in the raid, **including the headless client**, should run this mod, otherwise you will run into syncing issues.
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

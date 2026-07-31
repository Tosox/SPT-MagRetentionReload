# Mag Retention Reload

> A similar mag retention feature is already part of **UI Fixes** by **Tyfon**.  
> I originally started working on this mod without knowing that it existed there and by the time I found out, my version was already mostly finished.
>
> This mod keeps the behavior simple and does not add any reload time penalties which may be preferable depending on your setup.

## 📜 Description

**Mag Retention Reload** is a mod for *Single Player Tarkov* that adds proper magazine retention to weapon reloads by automatically stowing the old magazine back into the rig if possible instead of dropping it.

## ✨ Features

* Swaps the old magazine into the slot the new magazine was taken from
* Prevents unnecessary magazine drops during normal reloads if the magazine fits in the rig
* Works with both the reload hotkey and the inventory context menu

## 📁 Installation

* Download the latest release
* Copy the `BepInEx` folder into your SPT folder
* Start the game

## 🤝 Fika

When playing with **Fika**, every client in the raid, **including the headless client**, should run this mod, otherwise you will run into syncing issues.
Each client applies the retention itself while mirroring the other players' reloads. A client without the mod disagrees about where the old magazine ended up, which leaves magazines that look like they are lying on the ground but cannot be picked up.

Hosts can enforce this by adding the plugin GUID to `client.mods.required` in the Fika server config, so mismatched clients are rejected when joining instead of silently desyncing:

```jsonc
"required": [ "de.tosox.magretentionreload" ]
```

## 📷 Preview

<img src="readme-res/reload.gif" alt="reload" width="500"/>

## 🙏 Acknowledgements

Thanks to [**Lacyway**](https://github.com/Lacyway) for putting up with all my questions

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

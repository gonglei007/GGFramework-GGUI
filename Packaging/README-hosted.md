# GGUI hosted UGUI package

Local preview exported directly from the canonical GGUI source files. UIHost manages
instance-owned windows in existing layer transforms; UIBase/UIWindow provide their
lifecycle. UGUIImplementation supports UGUI/TMP text, buttons, pointer masks,
normalized progress, layout and unscaled fades. ModalFocusScope owns keyboard
focus and restores the previous selection.

The export does not compile the old UISystem, NGUI, AssetBundle resource backend,
Lua or third-party bundles. Those APIs remain in the original project and are not
claimed as migrated. Hosting existing layers does not change their sibling order,
canvas sorting, positions, scales, styles or resource ownership.

Initialize UIHost with an explicit IUIImplementation, register child transforms,
and Present a window already parented to its layer. Dispose its lease to close it.
Replacing a window retires its old lease before callbacks; stale leases cannot close
a replacement. CloseAll rejects new presentation during teardown, releases every
window and aggregates callback errors. Window objects belong to the host after
Present and are destroyed on close.

The legacy project installs a fallback resolver in Bridge/LegacyUIImplementation.
The hosted package has no hidden singleton fallback. Existing game components
that retain serialized GUIDs may derive from ModalFocusScope.

This preview is unpublished. Record the base commit, exact export manifest and
archive hash; a base commit alone does not contain uncommitted candidate changes.

# VRChat Dumper
VRChat Dumper is a tool that tries to find types and methods in obfuscated VRChat assemblies. It goes through every type and method in an assembly and checks if it matched any of the rules.

## How to use
You first need to dump VRChat's `GameAssembly.dll` using Il2CppDumper, and then just put the generated `Assembly-CSharp.dll` in the same folder as this program. After running, the result will be saved in `output.txt` in the following format:
```
Type ÌÏÎÍÌÌÍÎÎÏÍÍÎÏÎÎÍÏÏÎÎÎÌ matched with ActionMenu
Type ÍÍÏÍÌÌÏÏÍÎÌÍÌÏÎÏÌÎÎÌÏÎÎ matched with PedalOption
Type ÏÌÏÎÎÍÌÌÏÌÏÌÌÍÏÍÍÏÌÍÏÍÎ matched with VRCInput
(...)
Method ÍÍÏÍÌÌÏÏÍÎÌÍÌÏÎÏÌÎÎÌÏÎÎ.ÏÎÌÍÏÏÏÎÍÏÍÍÌÏÍÍÌÌÏÍÎÍÏ matched with PedalOption.OnClicked
Method ÍÍÏÍÌÌÏÏÍÎÌÍÌÏÎÏÌÎÎÌÏÎÎ.ÍÏÏÏÏÍÌÍÏÌÏÌÍÎÎÍÌÏÎÌÍÎÌ matched with PedalOption.SetName
```

## How to add rules
You can add new rules by editing `Program.cs`, simply add a new value to the `TypeRules` or `MethodRules` dictionary. Remember that to use `TypeCache` the actual type has to be found by `TypeRules` first. Feel free to create pull requests with any new rules.

# DotnetNoVirtualProtectShellcodeLoader
load shellcode without P/D Invoke and VirtualProtect call.

# How

This code leverages built-in .NET functionality to allocate an RWX memory region and overwrite a C# method with your own shellcode using the `RuntimeHelpers.PrepareMethod(handle)` method.

# Usage

The POC is remotely fetching the shellcode on a remote server (a pop calc x86)

# Credit 

Mr.Un1k0d3r 2025

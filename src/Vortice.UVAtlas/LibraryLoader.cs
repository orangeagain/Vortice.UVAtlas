// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vortice.UVAtlas
{
    public static class LibraryLoader
    {
        static LibraryLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Extension = ".dll";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Extension = ".dylib";
            else
                Extension = ".so";
        }

        public static string Extension { get; }

        public static string Rid
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Environment.Is64BitProcess ? "win-x64" : "win-x86";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Environment.Is64BitProcess ? "linux-x64" : "linux-x86";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "osx-x64";
                else
                    return "unknown";
            }
        }

        public static IntPtr LoadLocalLibrary(string libraryName)
        {
            var ret = IntPtr.Zero;
            string? assemblyLocation = Path.GetDirectoryName(typeof(LibraryLoader).Assembly.Location) ?? "./";

            // Try .NET Framework / mono locations
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, libraryName));

                // Look in Frameworks for .app bundles
                if (ret == IntPtr.Zero)
                    ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, "..", "Frameworks", libraryName));
            }
            else
            {
                if (Environment.Is64BitProcess)
                    ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, "x64", libraryName));
                else
                    ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, "x86", libraryName));
            }

            // Try .NET Core development locations
            if (ret == IntPtr.Zero)
            {
                ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, "native", Rid, libraryName));

                if (ret == IntPtr.Zero)
                    ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, "runtimes", Rid, "native", libraryName));
            }

            // Try current folder (.NET Core will copy it there after publish)
            if (ret == IntPtr.Zero)
                ret = LoadPlatformLibrary(Path.Combine(assemblyLocation, libraryName));

            // Try alternate way of checking current folder
            // assemblyLocation is null if we are inside macOS app bundle
            if (ret == IntPtr.Zero)
                ret = LoadPlatformLibrary(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, libraryName));

            // Try loading system library
            if (ret == IntPtr.Zero)
                ret = LoadPlatformLibrary(libraryName);

            // Welp, all failed, PANIC!!!
            if (ret == IntPtr.Zero)
                throw new Exception("Failed to load library: " + libraryName);

            return ret;
        }

        public static T LoadFunction<T>(IntPtr library, string name)
        {
            IntPtr symbol = GetSymbol(library, name);

            if (symbol == IntPtr.Zero)
                throw new EntryPointNotFoundException($"Unable to load symbol '{name}'.");

            return Marshal.GetDelegateForFunctionPointer<T>(symbol);
        }

        private static IntPtr LoadPlatformLibrary(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
                throw new ArgumentNullException(nameof(libraryName));

            IntPtr handle;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                handle = Win32.LoadLibrary(libraryName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                handle = Linux.dlopen(libraryName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                handle = Mac.dlopen(libraryName);
            else
                throw new PlatformNotSupportedException($"Current platform is unknown, unable to load library '{libraryName}'.");

            return handle;
        }

        private static IntPtr GetSymbol(IntPtr library, string symbolName)
        {
            if (string.IsNullOrEmpty(symbolName))
                throw new ArgumentNullException(nameof(symbolName));

            IntPtr handle;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                handle = Win32.GetProcAddress(library, symbolName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                handle = Linux.dlsym(library, symbolName);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                handle = Mac.dlsym(library, symbolName);
            else
                throw new PlatformNotSupportedException($"Current platform is unknown, unable to load symbol '{symbolName}' from library {library}.");

            return handle;
        }

        private static class Mac
        {
            private const string SystemLibrary = "/usr/lib/libSystem.dylib";

            private const int RTLD_LAZY = 1;
            private const int RTLD_NOW = 2;

            public static IntPtr dlopen(string path, bool lazy = true) =>
                dlopen(path, lazy ? RTLD_LAZY : RTLD_NOW);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlopen(string path, int mode);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(SystemLibrary)]
            public static extern void dlclose(IntPtr handle);
        }

        private static class Linux
        {
            private const string SystemLibrary = "libdl.so";

            private const int RTLD_LAZY = 1;
            private const int RTLD_NOW = 2;

            public static IntPtr dlopen(string path, bool lazy = true) =>
                dlopen(path, lazy ? RTLD_LAZY : RTLD_NOW);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlopen(string path, int mode);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport(SystemLibrary)]
            public static extern void dlclose(IntPtr handle);
        }

        private static class Win32
        {
            private const string SystemLibrary = "Kernel32.dll";

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern void FreeLibrary(IntPtr hModule);
        }
    }
}

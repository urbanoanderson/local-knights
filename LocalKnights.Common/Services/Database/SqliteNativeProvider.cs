using System.Runtime.InteropServices;
using SQLitePCL;

namespace LocalKnights.Common.Services.Database
{
    /// <summary>
    /// Configures SQLitePCLRaw to use the application-provided SQLite3MC native library.
    /// </summary>
    public static class SqliteNativeProvider
    {
        private const string NativeLibraryFileName = "sqlite3mc.dll";
        private static readonly object InitializationLock = new object();
        private static bool _isInitialized;
        private static IntPtr _nativeLibraryHandle;

        /// <summary>
        /// Loads SQLite3MC and registers it as the process-wide SQLitePCLRaw provider.
        /// </summary>
        public static void Initialize()
        {
            lock (InitializationLock)
            {
                if (_isInitialized)
                {
                    return;
                }

                string nativeLibraryPath = Path.Combine(AppContext.BaseDirectory, NativeLibraryFileName);
                if (!File.Exists(nativeLibraryPath))
                {
                    throw new FileNotFoundException("The SQLite3MC native library was not found.", nativeLibraryPath);
                }

                _nativeLibraryHandle = NativeLibrary.Load(nativeLibraryPath);
                DynamicFunctionPointerResolver resolver = new DynamicFunctionPointerResolver(_nativeLibraryHandle);
                SQLite3Provider_dynamic_cdecl.Setup("sqlite3mc", resolver);
                raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
                raw.FreezeProvider();
                _isInitialized = true;
            }
        }

        private sealed class DynamicFunctionPointerResolver : IGetFunctionPointer
        {
            private readonly IntPtr _nativeLibraryHandle;

            public DynamicFunctionPointerResolver(IntPtr nativeLibraryHandle)
            {
                _nativeLibraryHandle = nativeLibraryHandle;
            }

            public IntPtr GetFunctionPointer(string name)
            {
                return NativeLibrary.TryGetExport(_nativeLibraryHandle, name, out IntPtr address)
                    ? address
                    : IntPtr.Zero;
            }
        }
    }
}
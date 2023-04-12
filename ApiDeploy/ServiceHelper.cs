﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ApiDeploy;

public static class ServiceHelper {
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern Boolean ChangeServiceConfig(
        IntPtr hService,
        UInt32 nServiceType,
        UInt32 nStartType,
        UInt32 nErrorControl,
        String lpBinaryPathName,
        String lpLoadOrderGroup,
        IntPtr lpdwTagId,
        [In] char[] lpDependencies,
        String lpServiceStartName,
        String lpPassword,
        String lpDisplayName);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr OpenService(
        IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

    [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr OpenSCManager(
        string machineName, string databaseName, uint dwAccess);

    [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
    public static extern int CloseServiceHandle(IntPtr hSCObject);

    private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
    private const uint SERVICE_QUERY_CONFIG = 0x00000001;
    private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
    private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;
    
    private const uint SC_MANAGER_CONNECT = 0x0001;
    private const uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
    
    private const uint SERVICE_QUERY_STATUS = 0x0004;
    private const uint SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
    private const uint SERVICE_START = 0x0010;
    private const uint SERVICE_STOP = 0x0020;
    private const uint SERVICE_PAUSE_CONTINUE = 0x0040;
    private const uint SERVICE_INTERROGATE = 0x0080;
    private const uint SERVICE_USER_DEFINED_CONTROL = 0x0100;
    

    public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode) {
        var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_CONNECT | SERVICE_QUERY_CONFIG);
        if (scManagerHandle == IntPtr.Zero) {
            throw new ExternalException("Open Service Manager Error");
        }

        var serviceHandle = OpenService(
            scManagerHandle,
            svc.ServiceName,
            SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

        if (serviceHandle == IntPtr.Zero) {
            throw new ExternalException("Open Service Error");
        }

        var result = ChangeServiceConfig(
            serviceHandle,
            SERVICE_NO_CHANGE,
            (uint)mode,
            SERVICE_NO_CHANGE,
            null,
            null,
            IntPtr.Zero,
            null,
            null,
            null,
            null);

        if (result == false) {
            int nError = Marshal.GetLastWin32Error();
            var win32Exception = new Win32Exception(nError);
            throw new ExternalException("Could not change service start type: "
                                        + win32Exception.Message);
        }

        CloseServiceHandle(serviceHandle);
        CloseServiceHandle(scManagerHandle);
    }
}
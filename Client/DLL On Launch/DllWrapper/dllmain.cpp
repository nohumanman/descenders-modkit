#define _CRT_SECURE_NO_WARNINGS
#include "version/version.h"
#include <iostream>
#include <iostream>
#include <fstream>
#include <iostream>
#include <fstream>
#include <shlwapi.h>
#include <metahost.h>
#pragma comment(lib, "mscoree.lib")


#define ASSEMBLY_PATH "/ModLoaderSolution.dll"
#define PAYLOAD_NAMESPACE "MyNamespace"
#define PAYLOAD_CLASS "MyClass"
#define PAYLOAD_MAIN  "MyMethod"

typedef VOID MonoObject;
typedef VOID MonoDomain;
typedef VOID MonoAssembly;
typedef VOID MonoImage;
typedef VOID MonoClass;
typedef VOID MonoMethod;
typedef VOID MonoImageOpenStatus;

// typedefs and fields for required mono functions
typedef void(__cdecl* t_mono_thread_attach)(MonoDomain*);
t_mono_thread_attach fnThreadAttach;
typedef  MonoDomain* (__cdecl* t_mono_get_root_domain)(void);
t_mono_get_root_domain fnGetRootDomain;
typedef MonoAssembly* (__cdecl* t_mono_assembly_open)(const char*, MonoImageOpenStatus*);
t_mono_assembly_open fnAssemblyOpen;
typedef MonoImage* (__cdecl* t_mono_assembly_get_image)(MonoAssembly*);
t_mono_assembly_get_image fnAssemblyGetImage;
typedef MonoClass* (__cdecl* t_mono_class_from_name)(MonoImage*, const char*, const char*);
t_mono_class_from_name fnClassFromName;
typedef MonoMethod* (__cdecl* t_mono_class_get_method_from_name)(MonoClass*, const char*, int);
t_mono_class_get_method_from_name fnMethodFromName;
typedef MonoObject* (__cdecl* t_mono_runtime_invoke)(MonoMethod*, void*, void**, MonoObject**);
t_mono_runtime_invoke fnRuntimeInvoke;
typedef const char* (__cdecl* t_mono_assembly_getrootdir)(void);
t_mono_assembly_getrootdir fnGetRootDir;

void initMonoFunctions(HMODULE mono) {
    fnThreadAttach = (t_mono_thread_attach)GetProcAddress(mono, "mono_thread_attach");
    fnGetRootDomain = (t_mono_get_root_domain)GetProcAddress(mono, "mono_get_root_domain");
    fnAssemblyOpen = (t_mono_assembly_open)GetProcAddress(mono, "mono_assembly_open");
    fnAssemblyGetImage = (t_mono_assembly_get_image)GetProcAddress(mono, "mono_assembly_get_image");
    fnClassFromName = (t_mono_class_from_name)GetProcAddress(mono, "mono_class_from_name");
    fnMethodFromName = (t_mono_class_get_method_from_name)GetProcAddress(mono, "mono_class_get_method_from_name");
    fnRuntimeInvoke = (t_mono_runtime_invoke)GetProcAddress(mono, "mono_runtime_invoke");
    fnGetRootDir = (t_mono_assembly_getrootdir)GetProcAddress(mono, "mono_assembly_getrootdir");
}

void callCSharp() {
    HRESULT hr;
    ICLRMetaHost* pMetaHost = NULL;
    ICLRRuntimeInfo* pRuntimeInfo = NULL;
    ICLRRuntimeHost* pClrRuntimeHost = NULL;

    // Bind to the CLR runtime..
    hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));
    hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
    hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost,
        IID_PPV_ARGS(&pClrRuntimeHost));

    // Push the big START button shown above
    hr = pClrRuntimeHost->Start();

    // get local folder location

    char exePath[MAX_PATH];
    GetModuleFileNameA(NULL, exePath, MAX_PATH);
    PathRemoveFileSpecA(exePath);
    PathAppendA(exePath, "ModInjector.dll");
    //
    wchar_t wtext[MAX_PATH];
    mbstowcs(wtext, exePath, strlen(exePath) + 1);//Plus null
    LPWSTR ptr = wtext;

    // Okay, the CLR is up and running in this (previously native) process.
    // Now call a method on our managed C# class library.
    DWORD dwRet = 0;
    hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(
        ptr,
        L"ModInjector.ModInjector",
        L"Load",
        L"MyParameter",
        &dwRet
    );

    // Optionally stop the CLR runtime (we could also leave it running)
    //hr = pClrRuntimeHost->Stop();

    // Don't forget to clean up.
    pClrRuntimeHost->Release();
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
            setupWrappers();
            CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)callCSharp, NULL, 0, NULL);
            break;
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
        case DLL_PROCESS_DETACH:
            break;
    }
    return TRUE;
}


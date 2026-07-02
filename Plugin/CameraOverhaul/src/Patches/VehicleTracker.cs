using GameNetcodeStuff;
using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(VehicleController))]
internal static class VehicleTracker
{
    private static VehicleController? _active;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(VehicleController.Update))]
    private static void UpdatePostfix(VehicleController __instance)
    {
        if (!__instance.IsSpawned)
            return;
        // do not attemot to patch custom vehicles, the vanilla CC has an ID of 0, this will also "just work" out the box for the Version-55 Company Cruiser mod, the ScanVan and the Company Hauler without soft dep, as they uses the 'new' keyword.
        // this is good future proofing if you ever patch anything VehicleController, incase a creator uses base methods, and you only want to touch the vanilla CC
        if (__instance.vehicleID != 0) 
            return;   
        
        PlayerControllerB? lp = StartOfRound.Instance?.localPlayerController;
        if (IsPlayerInVehicle(lp, __instance))
            _active = __instance;
        else if (_active == __instance)
            _active = null;
    }

    public static VehicleController? ResolveActive(PlayerControllerB? localPlayer)
        => IsPlayerInVehicle(localPlayer, _active) ? _active : null;

    public static void ClearCachedActiveIf(PlayerControllerB? localPlayer)
    {
        if (!IsPlayerInVehicle(localPlayer, _active))
            _active = null;
    }

    public static void Reset() => _active = null;

    private static bool IsPlayerInVehicle(PlayerControllerB? localPlayer, VehicleController? vehicle)
    {
        if (localPlayer == null || vehicle == null)
            return false;

        return vehicle.currentDriver == localPlayer
            || vehicle.currentPassenger == localPlayer
            || vehicle.localPlayerInControl
            || vehicle.localPlayerInPassengerSeat;
    }
}

using System;
using System.Runtime.InteropServices;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin;

public class GameFunctions
{
    #region Delegates
    
    private delegate byte ItemActionUnlockedDelegate(IntPtr data);
    
    private delegate bool CardUnlockedDelegate(IntPtr a1, ushort card);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte TryOnDelegate(uint unknownCanEquip, uint itemBaseId, ulong stainColor, uint itemGlamourId, byte unknownByte);

    #endregion

    private AddressResolver Address { get; }
    
    #region Functions

    private readonly ItemActionUnlockedDelegate _itemActionUnlocked = null;
    
    private readonly CardUnlockedDelegate _cardUnlocked = null;

    internal readonly TryOnDelegate _tryOn = null;

    #endregion
    
    public GameFunctions(AddressResolver address)
    {
        this.Address = address;
        
        _itemActionUnlocked = Marshal.GetDelegateForFunctionPointer<ItemActionUnlockedDelegate>(Address.IsItemActionUnlocked);
        _cardUnlocked = Marshal.GetDelegateForFunctionPointer<CardUnlockedDelegate>(Address.IsCardUnlocked);
        _tryOn = Marshal.GetDelegateForFunctionPointer<TryOnDelegate>(Address.TryOn);
    }
    
    internal bool IsCardOwned(ushort cardId) {
        return _cardUnlocked(Address.CardUnlockedStatic, cardId);
    }

    internal unsafe bool ItemActionUnlocked(Item item) {
        var itemAction = item.ItemAction.Value;
        if (itemAction == null) {
            return false;
        }

        var type = itemAction.Type;

        var mem = Marshal.AllocHGlobal(256);
        *(uint*) (mem + 142) = itemAction.RowId;

        if (type == 25183) {
            *(uint*) (mem + 112) = item.AdditionalData;
        }

        var ret = _itemActionUnlocked(mem) == 1;

        Marshal.FreeHGlobal(mem);

        return ret;
    }
}
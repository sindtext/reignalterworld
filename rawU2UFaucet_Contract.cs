using UnityEngine;
using System.Collections;
using Eidolon.SmartContracts;

public static class rawU2UFaucetManager
{
    // This is your contract object, You can instantiate this in any other script if required.
    // You can access this contract with rawU2UFaucetManager.rawU2UFaucetContract = new SmartContract(rawU2UFaucetManager.Address, rawU2UFaucetManager.ABI)
    public static SmartContract rawU2UFaucetContract;

    // This is your contract address and be accessed with ContractManager.contractAddress
    public static string Address = "0xee801c5f1856fc724edffd012cfebdeca9a033cf";

    // This is your contract ABI and be accessed with ContractManager.contractABI
    public static string ABI = "[{\"type\":\"constructor\",\"stateMutability\":\"payable\",\"inputs\":[]},{\"type\":\"error\",\"name\":\"AccessControlBadConfirmation\",\"inputs\":[]},{\"type\":\"error\",\"name\":\"AccessControlUnauthorizedAccount\",\"inputs\":[{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\"},{\"type\":\"bytes32\",\"name\":\"neededRole\",\"internalType\":\"bytes32\"}]},{\"type\":\"error\",\"name\":\"AddressEmptyCode\",\"inputs\":[{\"type\":\"address\",\"name\":\"target\",\"internalType\":\"address\"}]},{\"type\":\"error\",\"name\":\"AddressInsufficientBalance\",\"inputs\":[{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\"}]},{\"type\":\"error\",\"name\":\"FailedInnerCall\",\"inputs\":[]},{\"type\":\"error\",\"name\":\"SafeERC20FailedOperation\",\"inputs\":[{\"type\":\"address\",\"name\":\"token\",\"internalType\":\"address\"}]},{\"type\":\"event\",\"name\":\"RoleAdminChanged\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\",\"indexed\":true},{\"type\":\"bytes32\",\"name\":\"previousAdminRole\",\"internalType\":\"bytes32\",\"indexed\":true},{\"type\":\"bytes32\",\"name\":\"newAdminRole\",\"internalType\":\"bytes32\",\"indexed\":true}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"RoleGranted\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\",\"indexed\":true},{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\",\"indexed\":true},{\"type\":\"address\",\"name\":\"sender\",\"internalType\":\"address\",\"indexed\":true}],\"anonymous\":false},{\"type\":\"event\",\"name\":\"RoleRevoked\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\",\"indexed\":true},{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\",\"indexed\":true},{\"type\":\"address\",\"name\":\"sender\",\"internalType\":\"address\",\"indexed\":true}],\"anonymous\":false},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"bytes32\",\"name\":\"\",\"internalType\":\"bytes32\"}],\"name\":\"DEFAULT_ADMIN_ROLE\",\"inputs\":[]},{\"type\":\"function\",\"stateMutability\":\"payable\",\"outputs\":[],\"name\":\"deAdmin\",\"inputs\":[{\"type\":\"address\",\"name\":\"oldAdmin\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"bytes32\",\"name\":\"\",\"internalType\":\"bytes32\"}],\"name\":\"getRoleAdmin\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"grantRole\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\"},{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"bool\",\"name\":\"\",\"internalType\":\"bool\"}],\"name\":\"hasRole\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\"},{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"renounceRole\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\"},{\"type\":\"address\",\"name\":\"callerConfirmation\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"nonpayable\",\"outputs\":[],\"name\":\"revokeRole\",\"inputs\":[{\"type\":\"bytes32\",\"name\":\"role\",\"internalType\":\"bytes32\"},{\"type\":\"address\",\"name\":\"account\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"payable\",\"outputs\":[],\"name\":\"setAdmin\",\"inputs\":[{\"type\":\"address\",\"name\":\"newAdmin\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"payable\",\"outputs\":[],\"name\":\"setOwner\",\"inputs\":[{\"type\":\"address\",\"name\":\"newOwner\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"payable\",\"outputs\":[],\"name\":\"stuckTokens\",\"inputs\":[{\"type\":\"address\",\"name\":\"token\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"view\",\"outputs\":[{\"type\":\"bool\",\"name\":\"\",\"internalType\":\"bool\"}],\"name\":\"supportsInterface\",\"inputs\":[{\"type\":\"bytes4\",\"name\":\"interfaceId\",\"internalType\":\"bytes4\"}]},{\"type\":\"function\",\"stateMutability\":\"payable\",\"outputs\":[],\"name\":\"u2uAdd\",\"inputs\":[{\"type\":\"address\",\"name\":\"faucet\",\"internalType\":\"address\"}]},{\"type\":\"function\",\"stateMutability\":\"payable\",\"outputs\":[],\"name\":\"u2uPay\",\"inputs\":[{\"type\":\"address\",\"name\":\"receiver\",\"internalType\":\"addresspayable\"}]}]";

    // Feel free to add to this manager with external contract pairs..
}
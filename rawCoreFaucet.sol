// SPDX-License-Identifier: NONE

pragma solidity 0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract rawCoreFaucet is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;
    address rawCoreFaucets;
    
    bytes32 internal constant MANAGER_ROLE = keccak256("MANAGER_ROLE");
    bytes32 internal constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    
    iKAIAFaucet iCoreFaucet;

    constructor() payable {
        _grantRole(MANAGER_ROLE, msg.sender);
        rawCoreFaucets = address(this);
    }

    function CoreAdd(address faucet) external payable onlyRole(ADMIN_ROLE) {
        iCoreFaucet = iKAIAFaucet(faucet);
    }
    
    function CorePay(address payable receiver) external payable onlyRole(ADMIN_ROLE) {
        iCoreFaucet.Pay(receiver);
    }

    function SetAdmin(address newadmin) external payable onlyRole(MANAGER_ROLE) {
        _grantRole(ADMIN_ROLE, newadmin);
    }

    function DeAdmin(address newadmin) external payable onlyRole(MANAGER_ROLE) {
        _revokeRole(ADMIN_ROLE, newadmin);
    }

    function SetManager(address newmanager) external payable onlyRole(MANAGER_ROLE) {
        _grantRole(MANAGER_ROLE, newmanager);
        _revokeRole(MANAGER_ROLE, _msgSender());
    }

    function StuckTokens(address token) external payable onlyRole(MANAGER_ROLE) {
        if (token == address(0x0)) {
            payable(_msgSender()).transfer(address(this).balance);
            return;
        }
        IERC20 _stucktoken = IERC20(token);
        _stucktoken.safeTransfer(_msgSender(), _stucktoken.balanceOf(rawCoreFaucets));
    }
}

interface iKAIAFaucet{
  function Pay(address payable receiver) external;
}

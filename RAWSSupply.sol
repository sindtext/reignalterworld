// SPDX-License-Identifier: NONE
pragma solidity 0.8.24;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Permit.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract RAWSSupply is ERC20, AccessControl, ERC20Permit, ReentrancyGuard {
    bytes32 public constant MINTER_ROLE = keccak256("MINTER_ROLE");

    uint256 maxSupply = 1600000000 * 10 ** decimals();

    struct Allocation {
        string Utility;
        address Entitled;
        uint256 Right;
        uint256 Balance;
        uint Unlock;
        uint Unlocked;
        uint Monthly;
        uint256 TimeStamp;
    }

    string[] internal utilities;

    mapping(string => Allocation) internal allocation;

    constructor()
        ERC20("Reign Alter World Sovereign", "RAWS")
        ERC20Permit("Reign Alter World Sovereign")
    {
        _grantRole(DEFAULT_ADMIN_ROLE, payable(msg.sender));
    }

    function Sender() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function allocate(string calldata utility, address entitled, uint256 amount, uint unlock, uint cliff, uint monthly) external onlyRole(DEFAULT_ADMIN_ROLE) {
        allocation[utility] = Allocation (
            utility, entitled, amount, amount, 0, unlock, monthly, block.timestamp + (cliff * 60 * 60 * 24 * 30)
        );

        utilities.push(utility);
        grantRole(MINTER_ROLE, entitled);
    }

    function disallocate(uint order) external onlyRole(DEFAULT_ADMIN_ROLE) {
        revokeRole(MINTER_ROLE, allocation[utilities[order]].Entitled);
        delete allocation[utilities[order]];
        delete utilities[order];
    }

    function takeRight(string calldata utility) external onlyRole(MINTER_ROLE) nonReentrant {
        require(Sender() == allocation[utility].Entitled, "Not Entitled");

        uint percent = utilityAllocation(utility);
        require(percent > 0, "Not Enough Utility Allocation");

        uint allotment = utilityAllotment(utility);
        require(allotment <= allocation[utility].Balance, "Not Enough Utility Balance");
        require(totalSupply() + allotment * 10 ** decimals() <= maxSupply, "Max Supply Reach");

        allocation[utility].Unlocked += percent;
        allocation[utility].Balance -= allotment;
        _mint(Sender(), allotment * 10 ** decimals());
    }

    function utilityList() external view returns (string[] memory) {
        return utilities;
    }

    function utilityCheck(string calldata utility) external view returns (Allocation memory) {
        return allocation[utility];
    }

    function utilityAllocation(string calldata utility) public view returns (uint) {
        uint counter = (block.timestamp - allocation[utility].TimeStamp) / 60 / 60 / 24 / 30;
        uint monthly = counter * allocation[utility].Monthly;
        uint percent = allocation[utility].Unlock + monthly - allocation[utility].Unlocked;
        return percent;
    }

    function utilityAllotment(string calldata utility) public view returns (uint256) {
        uint allotment = allocation[utility].Right * (utilityAllocation(utility)) / 1000;
        return allotment;
    }

    function claimStuckTokens(address token, uint256 amount) external onlyRole(DEFAULT_ADMIN_ROLE) {
        if (token == address(0x0)) {
            payable(Sender()).transfer(amount);
            return;
        }
        ERC20 ERC20token = ERC20(token);
        uint256 stuckBalance = ERC20token.balanceOf(address(this));
        ERC20token.transfer(Sender(), stuckBalance);
    }
}
// SPDX-License-Identifier: NONE

pragma solidity 0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Permit.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract RAWS is ERC20, AccessControl, ERC20Permit, ReentrancyGuard {
    constructor() payable
        ERC20("Reign Alter World Sovereign", "RAWS")
        ERC20Permit("Reign Alter World Sovereign")
    {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
    }

    bytes32 internal constant MINTER_ROLE = keccak256("MINTER_ROLE");

    uint256 internal maxSupply = 1600000000;

    struct Allocation {
        string Utility;
        address Entitled;
        uint256 Right;
        uint256 Balance;
        uint Unlock;
        uint Monthly;
        uint256 Cliff;
    }

    string[] internal utilities;
    uint256 internal allocated;
    uint256 internal taked;

    mapping(string => Allocation) internal allocation;
    
    event allocationUpdate(Allocation result);

    function _sender() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function allocate(string calldata utility, address entitled, uint256 right, uint cliff, uint unlock, uint monthly) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(allocation[utility].Entitled == address(0x0), "Already Exist");
        require(allocated + right <= maxSupply, "Max Supply Reach");
        
        allocation[utility] = Allocation (
            utility, entitled, right, right, unlock * 10, monthly * 10, block.timestamp + cliff * 60 * 60 * 24 * 30
        );

        allocated += right;
        utilities.push(utility);
        grantRole(MINTER_ROLE, entitled);

        emit allocationUpdate(allocation[utility]);
    }

    function reallocate(string calldata utility, address entitled, uint256 right, uint cliff, uint unlock, uint monthly) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(allocation[utility].Entitled != address(0x0), "Not Exist");
        require(allocated - allocation[utility].Right + right <= maxSupply, "Max Supply Reach");

        revokeRole(MINTER_ROLE, allocation[utility].Entitled);
        grantRole(MINTER_ROLE, entitled);

        allocated = allocated - allocation[utility].Right + right;

        allocation[utility].Monthly = monthly * 10;
        allocation[utility].Unlock = unlock * 10;
        allocation[utility].Cliff = block.timestamp + cliff * 60 * 60 * 24 * 30;
        allocation[utility].Balance = right - (allocation[utility].Right - allocation[utility].Balance);
        allocation[utility].Right = right;
        allocation[utility].Entitled = entitled;

        emit allocationUpdate(allocation[utility]);
    }

    function takeRight(string calldata utility, uint256 amount) external payable nonReentrant onlyRole(MINTER_ROLE) {
        require(_sender() == allocation[utility].Entitled, "Not Entitled");

        uint256 allotment = utilityAllotment(utility);
        uint256 unlock = allocation[utility].Right - allocation[utility].Balance;
        require(amount <= allotment - unlock, "Not Enough Utility Allocation");
        require(amount <= allocation[utility].Balance, "Not Enough Utility Balance");
        require(totalSupply() / 10 ** decimals() + amount <= maxSupply, "Max Supply Reach");

        allocation[utility].Balance -= amount;
        taked += amount;
        _mint(_sender(), amount * 10 ** decimals());

        emit allocationUpdate(allocation[utility]);
    }

    function utilityList() external view returns (string[] memory) {
        return utilities;
    }

    function utilityCheck(string calldata utility) external view returns (Allocation memory) {
        return allocation[utility];
    }

    function utilityAllotment(string calldata utility) public view returns (uint256) {
        uint256 monthly = 0;

        if(block.timestamp > allocation[utility].Cliff)
        {
            uint256 counter = (block.timestamp - allocation[utility].Cliff) / (60 * 60 * 24 * 30);
            monthly = counter * allocation[utility].Monthly;
        }

        uint256 percent = allocation[utility].Unlock + monthly;
        return allocation[utility].Right * percent / 1000;
    }

    function stuckTokens(address token) external onlyRole(DEFAULT_ADMIN_ROLE) {
        if (token == address(0x0)) {
            payable(_msgSender()).transfer(address(this).balance);
            return;
        }
        require(taked == totalSupply() / 10 ** decimals(), "Temporary Locked");
        
        ERC20 stucktoken = ERC20(token);
        uint256 stuckBalance = stucktoken.balanceOf(address(this));
        stucktoken.transfer(_sender(), stuckBalance);
    }
}
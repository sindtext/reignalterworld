// SPDX-License-Identifier: NONE

pragma solidity 0.8.20;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Permit.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract RAWBrz is ERC20, AccessControl, ERC20Permit, ReentrancyGuard {
    constructor() payable
        ERC20("Reign Alter World Bronze", "RAWBrz")
        ERC20Permit("Reign Alter World Bronze")
    {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
    }

    bytes32 internal constant MINTER_ROLE = keccak256("MINTER_ROLE");

    uint256 internal maxSupply = 128000000000000;

    struct Allocation {
        string Utility;
        address Entitled;
        uint256 Right;
        uint256 Balance;
    }

    string[] internal utilities;
    uint256 internal allocated;
    uint256 internal taked;

    mapping(string => Allocation) internal allocation;
    
    event allocationUpdate(Allocation result);

    function _sender() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function allocate(string calldata utility, address entitled) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(allocation[utility].Entitled == address(0x0), "Already Exist");
        require(allocated + maxSupply / 160 <= maxSupply, "Max Supply Reach");
        
        allocation[utility] = Allocation (
            utility, entitled, maxSupply / 160, maxSupply / 160
        );

        allocated += maxSupply / 160;
        utilities.push(utility);
        grantRole(MINTER_ROLE, entitled);

        emit allocationUpdate(allocation[utility]);
    }

    function reallocate(string calldata utility, address entitled) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(allocation[utility].Entitled != address(0x0), "Not Exist");

        revokeRole(MINTER_ROLE, allocation[utility].Entitled);
        grantRole(MINTER_ROLE, entitled);

        allocation[utility].Entitled = entitled;

        emit allocationUpdate(allocation[utility]);
    }

    function takeRight(string calldata utility, uint256 amount) external payable nonReentrant onlyRole(MINTER_ROLE) {
        require(_sender() == allocation[utility].Entitled, "Not Entitled");

        uint256 allotment = allocation[utility].Right;
        uint256 unlock = allotment - allocation[utility].Balance;
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
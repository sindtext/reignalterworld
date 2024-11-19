// SPDX-License-Identifier: NONE

pragma solidity 0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract TaikoFaucet is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;
    address rawtaiko;

    uint256 private amount = 0.0001 ether;

    constructor() payable {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        rawtaiko = address(this);
    }
    
    event Payed(address indexed payee, uint256 indexed amount, uint256 indexed timestamp);

    function _payer() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function _sender() internal view returns (address) {
        return msg.sender;
    }

    function getBalance(address payable receiver) public view returns (uint) {
        return receiver.balance;
    }

    function pay(address payable receiver) external payable nonReentrant {
        require(getBalance(_payer()) == 0, "TaikoFaucet: Caller must have no ETH");
        require(getBalance(payable(rawtaiko)) >= amount, "TaikoFaucet: Contract Empty");

        uint256 receiverBalance = receiver.balance;
        if (receiverBalance < amount) {
            uint256 payableAmount = amount - receiverBalance;
            receiver.transfer(payableAmount);
            emit Payed(receiver, payableAmount, block.timestamp);
        }
    }

    function depo() external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        require(_sender().balance > msg.value, "insuficient Balance");

        (bool sent, bytes memory data) = payable(rawtaiko).call{value: msg.value}("");
    }

    function updateAmount(uint256 _newAmount) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        require(_newAmount > 0, "TaikoFaucet: Invalid Amount");
        amount = _newAmount;
    }

    function setOwner(address newOwner) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        grantRole(DEFAULT_ADMIN_ROLE, newOwner);
        revokeRole(DEFAULT_ADMIN_ROLE, _payer());
    }

    function stuckTokens(address token) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        if (token == address(0x0)) {
            payable(_msgSender()).transfer(address(this).balance);
            return;
        }
        IERC20 ERC20token = IERC20(token);
        uint256 stuckBalance = ERC20token.balanceOf(rawtaiko);
        ERC20token.safeTransfer(_payer(), stuckBalance);
    }
}
// SPDX-License-Identifier: NONE

pragma solidity 0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract RawChanger is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;
    address rawchanger;

    struct Ticket {
        string Code;
        string Symbol;
        uint256 Balance;
    }
    
    iRawBank irawbank;

    mapping(string => string) internal officer;

    mapping(string => Ticket) internal ticket;

    event ticketUpdate(Ticket result);

    constructor() payable {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        rawchanger = address(this);
    }
    
    bytes32 internal constant ADMIN_ROLE = keccak256("ADMIN_ROLE");

    function _payer() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function _sender() internal view returns (address) {
        return msg.sender;
    }

    function rawExchange(string calldata code, string calldata uid, uint key, uint lock, uint256 amount) external nonReentrant {
        require(ticket[code].Balance == 1, "Expired");
        
        receiverAssistant(officer["exchanger"], ticket[code].Symbol, uid, key, lock, amount);
        emit ticketUpdate(ticket[code]);
    }

    function redeemTicket(string calldata code, string calldata uid, uint key, uint lock) external nonReentrant {
        require(ticket[code].Balance > 0, "Expired");
        
        receiverAssistant(officer["airdroper"], ticket[code].Symbol, uid, key, lock, ticket[code].Balance);
        emit ticketUpdate(ticket[code]);
    }

    function addRawBank(address rawbankaddress) external payable onlyRole(ADMIN_ROLE) {
        irawbank = iRawBank(rawbankaddress);
    }

    function setOfficer(string calldata ofc, string calldata uid) external payable onlyRole(ADMIN_ROLE) {
        officer[ofc] = uid;
    }

    function addTicket(string calldata code, string calldata symbol, uint256 amount) external payable onlyRole(ADMIN_ROLE) {
        ticket[code] = Ticket(
            code,
            symbol,
            amount
        );

        emit ticketUpdate(ticket[code]);
    }

    function setOwner(address newOwner) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        grantRole(DEFAULT_ADMIN_ROLE, newOwner);
        revokeRole(DEFAULT_ADMIN_ROLE, _payer());
    }

    function setAdmin(address newAdmin) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        grantRole(ADMIN_ROLE, newAdmin);
    }

    function deAdmin(address oldAdmin) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        revokeRole(ADMIN_ROLE, oldAdmin);
    }

    function stuckTokens(address token) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        if (token == address(0x0)) {
            payable(_msgSender()).transfer(address(this).balance);
            return;
        }
        IERC20 ERC20token = IERC20(token);
        uint256 stuckBalance = ERC20token.balanceOf(rawchanger);
        ERC20token.safeTransfer(_payer(), stuckBalance);
    }
    
    function receiverAssistant(string storage ofc, string storage symbol, string calldata uid, uint key, uint lock, uint256 amount) internal {
        irawbank.ReceiverAssistant(symbol, ofc, uid, _sender(), key, lock, amount);
    }
}

interface iRawBank{
  function ReceiverAssistant(string calldata symbol, string calldata uid, string calldata recipient, address add, uint key, uint lock, uint256 amount) external payable;
}
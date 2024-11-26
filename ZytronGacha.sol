// SPDX-License-Identifier: NONE

pragma solidity 0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract ZytronGacha is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;
    address gachaZytron;

    uint timeLimit;

    struct Presence {
        address userID;
        uint timeSign;
        uint outCome;
    }

    mapping(address => Presence) internal presence;

    constructor() payable {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        gachaZytron = address(this);
    }
    
    bytes32 internal constant ADMIN_ROLE = keccak256("ADMIN_ROLE");

    event gachaOutcome(uint result, uint times);

    function _payer() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function _sender() internal view returns (address) {
        return msg.sender;
    }

    function signGacha() external nonReentrant {
        require(presence[_sender()].userID == address(0x0), "Gacha Signed");

        presence[_sender()] = Presence (
            _sender(),
            block.timestamp - timeLimit,
            999999
        );
    }

    function gacha() external nonReentrant{
        require(presence[_sender()].userID != address(0x0), "Gacha Signed Yet");
        require(block.timestamp - presence[_sender()].timeSign > timeLimit, "Gacha Taked");

        bytes32 scrambler = keccak256(abi.encodePacked(msg.sender, block.timestamp, block.prevrandao));
        uint outcome = uint(scrambler) % 999999;
        
        presence[_sender()].timeSign = block.timestamp;
        presence[_sender()].outCome = outcome;

        emit gachaOutcome(presence[_sender()].outCome, presence[_sender()].timeSign);
    }

    function lastGacha() external view returns (uint) {
        return presence[_sender()].outCome;
    }

    function checkGacha() external view returns (bool) {
        return block.timestamp - presence[_sender()].timeSign > timeLimit;
    }

    function lastSign() external view returns (uint) {
        return presence[_sender()].timeSign;
    }

    function setTimeLimit(uint hourLimit) external payable onlyRole(ADMIN_ROLE) {
        timeLimit = hourLimit * 60 * 60;
    }

    function setAdmin(address newAdmin) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        grantRole(ADMIN_ROLE, newAdmin);
    }

    function deAdmin(address oldAdmin) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        revokeRole(ADMIN_ROLE, oldAdmin);
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
        uint256 stuckBalance = ERC20token.balanceOf(gachaZytron);
        ERC20token.safeTransfer(_payer(), stuckBalance);
    }
}
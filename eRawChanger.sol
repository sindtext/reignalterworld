// SPDX-License-Identifier: NONE

pragma solidity 0.8.20;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract eRawChanger is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;
    address erawchanger;

    struct Token{
        string Symbol;
        address tokenAddress;
        uint Idx;
    }

    struct Taxes {
        uint256 Tax;
        uint256 Balance;
    }

    struct Wallet {
        uint Lock;
        uint Keys;
        uint256 RAWS;
    }

    uint currentTokenIdx;
    uint[] internal chainList;

    mapping(string => Token) internal tokens;
    mapping(string => Taxes) internal taxes;
    mapping(address => Wallet) internal wallet;

    constructor() payable {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        erawchanger = address(this);
    }
    
    bytes32 internal constant ADMIN_ROLE = keccak256("ADMIN_ROLE");

    function _payer() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function _sender() internal view returns (address) {
        return msg.sender;
    }

    function getTaxes(string calldata name) internal view returns(Taxes memory){
        return taxes[name];
    }

    function getToken(string calldata name) internal view returns(Token memory){
        return tokens[name];
    }

    function Save(string calldata tkn, uint key, uint lock, uint256 amount) external nonReentrant {
        Token memory _tkn = getToken(tkn);

        require(_tkn.Idx != 0, "Wrong Token");

        Taxes memory _taxes = getTaxes(tkn);
        uint256 tkndecimal = ERC20(_tkn.tokenAddress).decimals();
        uint tknamnt = amount - amount * _taxes.Tax;

        require(IERC20(_tkn.tokenAddress).balanceOf(erawchanger) >= tknamnt * 10 ** tkndecimal, "Temporary Close");
        
        if(wallet[_sender()].Keys == 0)
        {
            wallet[_sender()] = Wallet (
                key,
                key,
                0
            );
        }

        require(wallet[_sender()].Keys == key, "Not Authorized");

        IERC20(_tkn.tokenAddress).safeTransfer(_payer(), tknamnt * 10 ** tkndecimal);
        
        uint _lock = (chainList[lock] + wallet[_sender()].Lock) % 999999;
        wallet[_sender()].Keys = _lock;
    }

    function exchange(string calldata nama, string calldata namb, uint256 amount) external nonReentrant {
        require(amount % 100 == 0, "Rate Not Match");

        Token memory _tkna = getToken(nama);
        Token memory _tknb = getToken(namb);

        require(_tkna.Idx != 0, "Wrong Token");
        require(_tknb.Idx != 0, "Wrong Token");
        require(_tknb.Idx > _tkna.Idx, "Wrong Token");

        Taxes memory _taxes = getTaxes(nama);
        uint256 decima = ERC20(_tkna.tokenAddress).decimals();
        uint256 decimb = ERC20(_tknb.tokenAddress).decimals();
        uint amnt = amount + amount * _taxes.Tax;
        uint bmnt = amount / 100;

        require(IERC20(_tkna.tokenAddress).balanceOf(_payer()) >= amnt * 10 ** decima, "insuficient Balance");
        require(IERC20(_tknb.tokenAddress).balanceOf(erawchanger) >= bmnt * 10 ** decimb, "Temporary Close");

        IERC20(_tkna.tokenAddress).safeTransferFrom(_payer(), erawchanger, amnt * 10 ** decima);
        IERC20(_tknb.tokenAddress).safeTransfer(_payer(), bmnt * 10 ** decimb);
    }

    function crossChange(string calldata tkn, uint256 amount) external nonReentrant{
        Token memory _tkn = getToken(tkn);

        require(_tkn.Idx != 0, "Wrong Token");
        require(_tkn.Idx == currentTokenIdx, "Wrong Token");

        Taxes memory _taxes = getTaxes(tkn);
        uint256 tkndecimal = ERC20(_tkn.tokenAddress).decimals();
        uint tknamnt = amount + amount * _taxes.Tax;

        require(IERC20(_tkn.tokenAddress).balanceOf(_payer()) >= tknamnt * 10 ** tkndecimal, "insuficient Balance");

        IERC20(_tkn.tokenAddress).safeTransferFrom(_payer(), erawchanger, tknamnt * 10 ** tkndecimal);
        wallet[_sender()].RAWS = amount / 100;
    }

    function getRAWS() external view returns(uint256){
        return wallet[_sender()].RAWS;
    }

    function useRAWS(uint256 amount) external nonReentrant{
        uint256 raws = wallet[_sender()].RAWS;

        require(raws >= amount, "insuficient Balance");

        wallet[_sender()].RAWS = amount - raws;
    }

    function addToken(string calldata name, string calldata symbol, address tokenaddress, uint256 tax) external payable onlyRole(ADMIN_ROLE) {
        currentTokenIdx++;
        tokens[name] = Token(
            symbol,
            tokenaddress,
            currentTokenIdx
        );

        taxes[name] = Taxes(
            tax,
            0
        );
    }

    function CheckChainList(uint idx) external onlyRole(ADMIN_ROLE) view returns (uint) {
        return chainList[idx];
    }

    function addLock() external payable onlyRole(ADMIN_ROLE) {
        bytes32 lock = keccak256(abi.encodePacked(msg.sender, block.timestamp, block.prevrandao));
        uint key = uint(lock) % 999999;
        
        chainList.push(key);
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
        uint256 stuckBalance = ERC20token.balanceOf(erawchanger);
        ERC20token.safeTransfer(_payer(), stuckBalance);
    }
}
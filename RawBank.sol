// SPDX-License-Identifier: NONE

pragma solidity 0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

contract RawBank is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;
    address rawbank;

    struct Token{
        string Symbol;
        string Name;
        address tokenAddress;
        uint256 userDeposit;
    }

    struct TokenOrder{
        uint256 Order;
    }

    struct Account {
        address Wallet;
        uint Keys;
        uint Lock;
        uint256[] Balance;
    }

    struct Wallet {
        string Account;
        uint Keys;
    }

    struct Assistant {
        address Wallet;
    }

    uint256 internal currentTokenOrder;
    uint[] internal chainList;
    
    mapping(string => Token) internal tokens;

    mapping(string => TokenOrder) internal tokenorder;

    mapping(string => Account) internal account;

    mapping(address => Wallet) internal wallet;

    mapping(string => Assistant) internal assistant;

    event accountUpdate(address wallet, string id, uint256[] balances);
    event balanceUpdate(string status, string holder, uint256 amount, uint256 balance);

    constructor() payable {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        rawbank = address(this);
    }

    bytes32 internal constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 internal constant AI_ROLE = keccak256("AI_ROLE");

    function _payer() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function _sender() internal view returns (address) {
        return msg.sender;
    }

    function getToken(string calldata symbol) internal view returns(Token memory){
        return tokens[symbol];
    }

    function checkAuth(string calldata uid, address add, uint key) internal view returns (bool){
        return account[uid].Keys == key && wallet[add].Keys == key;
    }

    function locking(string calldata uid, address add, uint lock) internal{
        uint _keys = (chainList[lock] + account[uid].Lock) % 999999;
        account[uid].Keys = _keys;
        wallet[add].Keys = _keys;
    }

    function CheckBalance(string calldata symbol, string calldata uid) external view returns (uint256) {
        return account[uid].Balance[tokenorder[symbol].Order];
    }

    function CreateAccount(string calldata uid, uint key, uint lock) external nonReentrant {
        require(account[uid].Keys == 0, "Account Already Exist");
        require(wallet[_sender()].Keys == 0, "Wallet Already Use");

        uint _keys = (chainList[lock] + key) % 999999;

        account[uid] = Account (
            _sender(),
            _keys,
            key,
            new uint256[](8)
        );

        wallet[_sender()] = Wallet (
            uid,
            _keys
        );

        emit accountUpdate(account[uid].Wallet, uid, account[uid].Balance);
    }

    function DepositBalance(string calldata symbol, string calldata uid, uint key, uint lock, uint256 amount) external nonReentrant {
        require(checkAuth(uid, _sender(), key), "Not Authorized");
        
        Token memory _token = getToken(symbol);
        uint256 tokendecimal = ERC20(_token.tokenAddress).decimals();
        require(IERC20(_token.tokenAddress).balanceOf(_payer()) >= amount * 10 ** tokendecimal, "insuficient Balance");

        IERC20(_token.tokenAddress).safeTransferFrom(_payer(), rawbank, amount * 10 ** tokendecimal);

        tokens[symbol].userDeposit += amount;
        uint256 blnc = account[uid].Balance[tokenorder[symbol].Order] + amount;
        account[uid].Balance[tokenorder[symbol].Order] = blnc;

        locking(uid, _sender(), lock);
        emit balanceUpdate("Deposit", uid, amount, blnc);
    }

    function RedeemBalance(string calldata symbol, string calldata uid, uint key, uint lock, uint256 amount) external nonReentrant {
        require(checkAuth(uid, _sender(), key), "Not Authorized");
        
        Token memory _token = getToken(symbol);

        require(account[uid].Balance[tokenorder[symbol].Order] >= amount, "Insufficient Balance");
        uint256 tokendecimal = ERC20(_token.tokenAddress).decimals();
        require(IERC20(_token.tokenAddress).balanceOf(rawbank) > amount * 10 ** tokendecimal, "Please Contact RAWBank Admin");

        tokens[symbol].userDeposit -= amount;
        uint256 blnc = account[uid].Balance[tokenorder[symbol].Order] - amount;
        account[uid].Balance[tokenorder[symbol].Order] = blnc;

        IERC20(_token.tokenAddress).safeTransfer(_payer(), amount * 10 ** tokendecimal);

        locking(uid, _sender(), lock);
        emit balanceUpdate("Redeem", uid, amount, blnc);
    }

    function TransferBalance(string calldata symbol, string calldata uid, uint key, uint lock, string calldata recipient, uint256 amount) external nonReentrant {
        require(checkAuth(uid, _sender(), key), "Not Authorized");

        require(account[uid].Balance[tokenorder[symbol].Order] >= amount, "Insufficient Balance");
        uint256 sBlnc = account[uid].Balance[tokenorder[symbol].Order] - amount;
        account[uid].Balance[tokenorder[symbol].Order] = sBlnc;

        uint256 rBlnc = account[recipient].Balance[tokenorder[symbol].Order] + amount;
        account[recipient].Balance[tokenorder[symbol].Order] = rBlnc;

        locking(uid, _sender(), lock);
        emit balanceUpdate("Transfer", recipient, amount, sBlnc);
    }

    function EnableAssistant(string calldata uid, uint key, uint lock) external nonReentrant {
        require(checkAuth(uid, _sender(), key), "Not Authorized");

        locking(uid, _sender(), lock);
        assistant[uid] = Assistant (
            _sender()
        );
    }

    function DisableAssistant(string calldata uid, uint key, uint lock) external nonReentrant {
        require(checkAuth(uid, _sender(), key), "Not Authorized");

        locking(uid, _sender(), lock);
        assistant[uid] = Assistant (
            address(0x0)
        );
    }

    function ReceiverAssistant(string calldata symbol, string calldata uid, string calldata recipient, address add, uint key, uint lock, uint256 amount) external onlyRole(AI_ROLE) nonReentrant {
        require(checkAuth(recipient, add, key), "Not Authorized");

        require(assistant[uid].Wallet != address(0x0), "Not Assisted");
        require(account[uid].Balance[tokenorder[symbol].Order] >= amount, "Please Contact RAWBank Admin");

        uint256 sBlnc = account[uid].Balance[tokenorder[symbol].Order] - amount;
        account[uid].Balance[tokenorder[symbol].Order] = sBlnc;

        uint256 rBlnc = account[recipient].Balance[tokenorder[symbol].Order] + amount;
        account[recipient].Balance[tokenorder[symbol].Order] = rBlnc;

        locking(recipient, add, lock);
        emit balanceUpdate("Assistant", recipient, amount, rBlnc);
    }

    function SecureAccount(string calldata uid, uint key, uint lock) external nonReentrant {
        require(account[uid].Keys == wallet[_sender()].Keys, "Not Your Account");

        uint _keys = (chainList[lock] + key) % 999999;

        account[uid].Keys = _keys;
        wallet[_sender()].Keys = _keys;

        emit accountUpdate(account[uid].Wallet, uid, account[uid].Balance);
    }

    function ChangeWallet(string calldata uid, uint key, uint lock) external nonReentrant {
        require(account[uid].Keys == key, "Not Your Account");

        require(wallet[_sender()].Keys == 0, "Wallet Already Use");

        address oldWallet = account[uid].Wallet;
        
        wallet[_sender()] = Wallet (
            uid,
            key
        );

        wallet[oldWallet].Keys = 0;
        wallet[oldWallet].Account = "";

        account[uid].Wallet = _sender();

        locking(uid, _sender(), lock);
        emit accountUpdate(account[uid].Wallet, uid, account[uid].Balance);
    }

    function SwitchAccount(string calldata uid, uint key, uint lock) external nonReentrant {
        require(wallet[_sender()].Keys == key, "Not Your Account");

        require(account[uid].Keys == 0, "Account Already Exist");

        string storage oldUID = wallet[_sender()].Account;

        account[uid] = Account (
            _sender(),
            key,
            account[oldUID].Lock,
            account[oldUID].Balance
        );

        account[oldUID].Wallet = address(0x0);
        account[oldUID].Keys = 0;
        account[oldUID].Lock = 0;
        account[oldUID].Balance = new uint256[](8);

        wallet[_sender()].Account = uid;

        locking(uid, _sender(), lock);
        emit accountUpdate(account[uid].Wallet, uid, account[uid].Balance);
    }

    function CheckAccount(string calldata uid) external onlyRole(ADMIN_ROLE) view returns (Account memory) {
        return account[uid];
    }

    function CheckChainList(uint idx) external onlyRole(ADMIN_ROLE) view returns (uint) {
        return chainList[idx];
    }

    function addLock() external payable onlyRole(ADMIN_ROLE) {
        bytes32 lock = keccak256(abi.encodePacked(msg.sender, block.timestamp, block.prevrandao));
        uint key = uint(lock) % 999999;
        
        chainList.push(key);
    }

    function addToken(string calldata name, string calldata symbol, address tokenaddress) external payable onlyRole(ADMIN_ROLE) {
        tokens[symbol] = Token(
            symbol,
            name,
            tokenaddress,
            0
        );

        tokenorder[symbol] = TokenOrder(
            currentTokenOrder
        );

        currentTokenOrder += 1;
    }

    function setAI(address newAI) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        grantRole(AI_ROLE, newAI);
    }

    function deAI(address oldAI) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        revokeRole(AI_ROLE, oldAI);
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

    function stuckTokens(string calldata symbol, address token) external payable onlyRole(DEFAULT_ADMIN_ROLE) {
        if (token == address(0x0)) {
            payable(_msgSender()).transfer(address(this).balance);
            return;
        }
        Token memory _token =  getToken(symbol);

        require(_token.tokenAddress == token, "Temporary Locked");
        IERC20 ERC20token = IERC20(token);
        uint256 stuckBalance = ERC20token.balanceOf(rawbank);

        require(_token.userDeposit < stuckBalance, "Temporary Locked");
        ERC20token.safeTransfer(_payer(), stuckBalance - _token.userDeposit);
    }
}
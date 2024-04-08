// SPDX-License-Identifier: NONE
pragma solidity 0.8.24;

import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/utils/math/SafeMath.sol";

contract RawBankAccount is ReentrancyGuard {
    using SafeERC20 for IERC20;
    address owner;
    address admin;

    struct Token{
        uint tokenId;
        string name;
        string symbol;
        address tokenAddress;
        uint apy;
    }

    struct Account {
        address Wallet;
        uint Keys;
        uint256 Balance;
    }

    struct Debit {
        string ID;
        uint256 Amount;
        uint256 TimeStamp;
    }

    struct Credit {
        string ID;
        uint256 Amount;
        uint256 TimeStamp;
    }
    
    uint public currentTokenId = 1;
    string[] public tokenSymbols;
    Token tokenRedeem;
    mapping(string => Token) public tokens;

    mapping(string => Account) internal account;
    mapping(string => Debit[]) internal debit;
    mapping(string => Credit[]) internal credit;

    event ownershipTransferred(address oldOwner, address newOwner);
    event adminTransferred(address oldAdmin, address newAdmin);
    event accountCreated(string id, uint256 balance);
    event balanceUpdate(string status, string holder, uint amount, uint256 balance);
    event accountScured(address wallet, string id, uint256 balance);
    event walletChanged(address wallet, string id, uint256 balance);
    event switchSucceed(address wallet, string id, uint256 balance);
    event readAccount(address wallet, string id, uint keys, uint256 balance);

    constructor() {
        owner = msg.sender;
    }

    modifier onlyOwner{
        require(owner == Payer(), "Only Owner Auth");
        _;
    }

    modifier onlyAdmin{
        require(admin == Payer() || owner == Payer(), "Only Admin Auth");
        _;
    }

    function Payer() internal view returns (address payable) {
        return payable(msg.sender);
    }

    function Sender() internal view returns (address) {
        return msg.sender;
    }

    function setOwner(address newOwner) external onlyOwner {
        address oldOwner = owner;
        owner = newOwner;
        emit ownershipTransferred(oldOwner, newOwner);
    }

    function setAdmin(address newAdmin) external onlyOwner {
        address oldAdmin = admin;
        admin = newAdmin;
        emit adminTransferred(oldAdmin, newAdmin);
    }

    function setTokenRedeem(string calldata symbol) external onlyAdmin {
        tokenRedeem = getToken(symbol);
    }

    function getTokenSymbol() external view returns(string[] memory){
        return tokenSymbols;
    }

    function getToken(string calldata tokenSymbol) public view returns(Token memory){
        return tokens[tokenSymbol];
    }

    function addToken(
        string calldata name, string calldata symbol, address tokenAddress, uint apy ) external onlyAdmin {
        tokenSymbols.push(symbol);
        tokens[symbol] = Token(
            currentTokenId,
            name,
            symbol,
            tokenAddress,
            apy
        );

        currentTokenId += 1;
    }

    function CreateAccount(string calldata symbol, string calldata uid, uint key, uint256 amount) public payable nonReentrant {
        Token memory _token =  getToken(symbol);
        address payable _payer = Payer();
        require(account[uid].Wallet != _payer, "Account Already Exist");

        if(amount > 0)
        {
            require(IERC20(_token.tokenAddress).balanceOf(_payer) > amount, "insuficient Balance");

            IERC20(_token.tokenAddress).safeTransferFrom(_payer, address(this), amount);
            _receive(uid, uid, amount);
        }

        account[uid] = Account (
            _payer,
            key,
            amount
        );

        emit accountCreated(uid, account[uid].Balance);
    }

    function CheckBalance(string calldata uid, uint key) external view returns (uint256) {
        require(account[uid].Wallet == Sender(), "Wrong Account");
        require(account[uid].Keys == key, "Not Authorized");

        uint256 myBalance = account[uid].Balance;

        return myBalance;
    }

    function AddBalance(string calldata uid, uint256 amount) external onlyAdmin {
        require(account[uid].Wallet != address(0x0), "Account Not Found");

        account[uid].Balance += amount;
        _receive(uid, uid, amount);

        emit balanceUpdate("Added", uid, amount, account[uid].Balance);
    }

    function MoveBalance(string memory uid, uint256 amount) internal {
        require(account[uid].Wallet != address(0x0), "Account Not Found");

        account[uid].Balance += amount;

        emit balanceUpdate("Move", uid, amount, account[uid].Balance);
    }

    function RemoveBalance(string calldata uid, uint256 amount) external onlyAdmin {
        require(account[uid].Wallet != address(0x0), "Account Not Found");
        require(account[uid].Balance >= amount, "insuficient Balance");

        account[uid].Balance -= amount;
        _send(uid, uid, amount);

        emit balanceUpdate("Remove", uid, amount, account[uid].Balance);
    }

    function DepositBalance(string calldata symbol, string calldata uid, uint key, uint lock, uint256 amount) external payable nonReentrant {
        Token memory _token =  getToken(symbol);
        address payable _payer = Payer();
        require(account[uid].Wallet == _payer, "Wrong Account");
        require(account[uid].Keys == key, "Not Authorized");
        require(IERC20(_token.tokenAddress).balanceOf(_payer) > amount, "insuficient Balance");

        IERC20(_token.tokenAddress).safeTransferFrom(_payer, address(this), amount);

        account[uid].Balance += amount;
        account[uid].Keys = lock;
        _receive(uid, uid, amount);

        emit balanceUpdate("Deposit", uid, amount, account[uid].Balance);
    }

    function TransferBalance(string calldata uid, uint key, uint lock, string calldata recipient, uint256 amount) external nonReentrant {
        address _sender = Sender();
        require(account[uid].Wallet == _sender, "Wrong Account");
        require(account[uid].Keys == key, "Not Authorized");
        require(account[uid].Balance >= amount, "Insufficient Balance");

        uint sBlnc = account[uid].Balance - amount;
        account[uid].Balance = sBlnc;

        uint rBlnc = account[uid].Balance + amount;
        account[uid].Balance = rBlnc;

        account[uid].Keys = lock;
        _send(uid, recipient, amount);
        _receive(uid, recipient, amount);

        emit balanceUpdate("Transfer", recipient, amount, sBlnc);
    }

    function RedeemBalance(string calldata uid, uint key, uint lock, uint256 amount) external payable nonReentrant {
        address payable _payer = Payer();
        require(account[uid].Wallet == _payer, "Wrong Account");
        require(account[uid].Keys == key, "Not Authorized");
        require(account[uid].Balance >= amount, "Insufficient Balance");
        require(IERC20(tokenRedeem.tokenAddress).balanceOf(address(this)) > amount, "Please Contact RAWBank Admin");

        IERC20(tokenRedeem.tokenAddress).safeTransfer(_payer, amount);

        account[uid].Balance -= amount;
        account[uid].Keys = lock;
        _send(uid, uid, amount);

        emit balanceUpdate("Redeem", uid, amount, account[uid].Balance);
    }

    function _send(string calldata sender, string calldata recipient, uint256 amount) internal {
        debit[sender].push(Debit(
            recipient,
            amount,
            block.timestamp
        ));
    }

    function _receive(string calldata sender, string calldata recipient, uint256 amount) internal {
        credit[recipient].push(Credit(
            sender,
            amount,
            block.timestamp
        ));
    }

    function ScureAccount(string calldata uid, uint lock) external nonReentrant {
        address _sender = Sender();
        require(account[uid].Wallet == _sender, "Wrong Account");

        account[uid].Keys = lock;

        emit accountScured(account[uid].Wallet, uid, account[uid].Balance);
    }

    function ChangeWallet(string calldata uid, uint key, address oldWallet) external nonReentrant {
        address _sender = Sender();
        require(account[uid].Keys == key, "Not Authorized");
        require(account[uid].Wallet == oldWallet, "Not Your Account");

        account[uid].Wallet = _sender;

        emit walletChanged(account[uid].Wallet, uid, account[uid].Balance);
    }

    function SwitchAccount(string calldata uid, uint key, string calldata oldID) external nonReentrant {
        address _sender = Sender();
        require(account[uid].Wallet == _sender, "Wrong Account");
        require(account[uid].Keys == key, "Not Authorized");
        require(account[uid].Wallet == account[oldID].Wallet, "Not Your Account");

        account[uid].Balance = account[oldID].Balance;

        account[oldID].Wallet = address(0x0);
        account[oldID].Keys = 0;
        account[oldID].Balance = 0;

        emit switchSucceed(account[uid].Wallet, uid, account[uid].Balance);
    }

    function CheckAccount(string calldata uid) external onlyAdmin {
        address myWallet = account[uid].Wallet;
        uint myKeys = account[uid].Keys;
        uint256 myBalance = account[uid].Balance;

        emit readAccount(myWallet, uid, myKeys, myBalance);
    }

    function claimStuckTokens(address token, uint256 amount) external onlyOwner {
        address payable _payer = Payer();
        if (token == address(0x0)) {
            payable(_payer).transfer(amount);
            return;
        }
        IERC20 ERC20token = IERC20(token);
        uint256 stuckBalance = ERC20token.balanceOf(address(this));
        ERC20token.safeTransfer(_payer, stuckBalance);
    }
}
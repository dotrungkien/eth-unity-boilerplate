using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;
using Nethereum.Hex.HexTypes;

public class SimpleStore : MonoBehaviour
{

    [Header("Deployed contract")]
    public TextAsset contractABI;
    public TextAsset contractAddress;

    public Text currentValueText;
    public Text addressText;
    public Text balanceText;
    public InputField inputValue;

    private HexBigInteger ethBalance;
    private Web3 web3;
    private Account account;
    private string privateKey;
    private string from;
    private Contract contract;

    private HexBigInteger gas = new HexBigInteger(900000);

    private Function getFunction;
    private Function setFunction;

    void Start()
    {
        AccountSetup();
    }

    void CopyToClipboard(string s)
    {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
    }

    public void CopyAddress()
    {
        CopyToClipboard(from);
    }

    public void CopyPrivateKey()
    {
        CopyToClipboard(privateKey);
    }

    public void AccountSetup()
    {
        var url = "http://localhost:8545";
        privateKey = "0x26b3f59a6fec532ffc45f121bd2ba3c088666a34136669df03b315e938330d58";
        account = new Account(privateKey);
        from = account.Address;
        web3 = new Web3(account, url);
        StartCoroutine(BalanceInterval());
        GetContract();
    }

    IEnumerator BalanceInterval()
    {
        while (true)
        {
            UpdateBalance();
            yield return new WaitForSeconds(1f);
        }
    }

    public async Task UpdateBalance()
    {
        var newBalance = await web3.Eth.GetBalance.SendRequestAsync(from);
        ethBalance = newBalance;
        decimal ethBalanceVal = Web3.Convert.FromWei(ethBalance.Value);
        addressText.text = from;
        balanceText.text = string.Format("{0:0.00} ETH", ethBalanceVal);
    }

    public async Task OnSetValue()
    {
        var newValue = inputValue.text;
        try
        {
            int intValue = Int32.Parse(newValue);
            await SetValue(intValue);
            int newValFromContract = await GetValue();
            currentValueText.text = "" + newValFromContract;

        }
        catch (FormatException)
        {
            Debug.LogError("Unable to parse input!");
        }
    }

    void GetContract()
    {
        string abi = contractABI.ToString();
        string address = contractAddress.ToString();
        contract = web3.Eth.GetContract(abi, address);

        getFunction = contract.GetFunction("set");
        setFunction = contract.GetFunction("get");
    }

    public async Task<int> GetValue()
    {
        var value = await getFunction.CallAsync<int>(from);
        return value;
    }

    public async Task<string> SetValue(int value)
    {
        var receipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, null, null, value);
        Debug.LogFormat("tx: {0}", receipt.TransactionHash);
        return receipt.TransactionHash;
    }
}

const fs = require('fs');
const path = require('path');
const contracts = path.resolve(__dirname, '../build/contracts/');
const unityAbis = path.resolve(__dirname, '../../game/Assets/Contracts/');

module.exports = function() {
  let builtContracts = fs.readdirSync(contracts);
  builtContracts.forEach(contract => {
    if (contract === 'Migrations.json' || contract === 'SafeMath.json') return;
    console.log('extracting contract ', contract);
    const name = contract.split('.')[0];
    let json = JSON.parse(fs.readFileSync(path.resolve(contracts, contract)));
    let { abi, networks } = json;
    if (!Object.keys(networks).length) return;
    fs.writeFileSync(path.resolve(unityAbis, contract), JSON.stringify(json.abi));
    fs.writeFileSync(path.resolve(unityAbis, name + 'Address.txt'), networks['123456789'].address);
  });
};

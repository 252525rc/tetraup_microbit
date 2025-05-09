var blePlugin = {
    // �ڑ�����Bluetooth�f�o�C�X
    $devices: {},

    // Bluetooth�f�o�C�X�ɐڑ������ǂ�����Ԃ��܂�
    IsConnected: function (targetName) {
        console.log('>>> isConnect');

        var target = Pointer_stringify(targetName);
        console.log('target:' + target);

        var result = false;

        if (devices[target]) {
            console.log('device:' + devices[target]);
            result = devices[target].gatt.connected;
        }

        console.log('<<< isConnect');

        return result;
    },

    // Bluetooth�f�o�C�X�ɐڑ����܂�
    Connect: function (targetName) {
        console.log('>>> connect');

        var target = Pointer_stringify(targetName);
        console.log('target:' + target);

        var ACCELEROMETER_SERVICE_UUID = 'e95d0753-251d-470a-a062-fa1922dfa9a8';
        var ACCELEROMETER_DATA_CHARACTERISTIC_UUID = 'e95dca4b-251d-470a-a062-fa1922dfa9a8';
        var ACCELEROMETER_PERIOD_CHARACTERISTIC_UUID = 'e95dfb24-251d-470a-a062-fa1922dfa9a8';
        var BUTTON_SERVICE_UUID = 'e95d9882-251d-470a-a062-fa1922dfa9a8';
        var BUTTON_A_STATE_CHARACTERISTIC_UUID = 'e95dda90-251d-470a-a062-fa1922dfa9a8';
        var BUTTON_B_STATE_CHARACTERISTIC_UUID = 'e95dda91-251d-470a-a062-fa1922dfa9a8';

        var bluetoothServer;
        var accelerometerService;
        var buttonService;

        // Bluetooth�f�o�C�X���擾���܂�
        var options = {
            filters: [
                { namePrefix: 'BBC micro:bit' }
            ],
            optionalServices: [ACCELEROMETER_SERVICE_UUID, BUTTON_SERVICE_UUID]
        };
        navigator.bluetooth.requestDevice(options)
            .then(function (device) {
                console.log('id:' + device.id);
                console.log('name:' + device.name);

                // �ڑ����؂ꂽ��ʒm���󂯎��܂�
                device.addEventListener('gattserverdisconnected', function (e) {
                    console.log('gattserverdisconnected');
                    SendMessage(target, 'OnDisconnected');
                });

                // �f�o�C�X�ɐڑ����܂�
                return device.gatt.connect();
            })
            .then(function (server) {
                console.log('connected.');
                devices[target] = server.device;

                // �����x�v�T�[�r�X���擾���܂�
                bluetoothServer = server;
                return bluetoothServer.getPrimaryService(ACCELEROMETER_SERVICE_UUID);
            })
            .then(function (service) {
                console.log('getPrimaryService');

                accelerometerService = service;
                return accelerometerService.getCharacteristic(ACCELEROMETER_PERIOD_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // �����x�v�̒l�̎擾�Ԋu��ݒ肵�܂�
                var period = new Uint16Array([20]);
                return characteristic.writeValue(period);
            })
            .then(function () {
                console.log('writeValue');

                return accelerometerService.getCharacteristic(ACCELEROMETER_DATA_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // �����x�v�̒l�̎擾���J�n���܂�
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                // �����x�v�̒l���󂯎��܂�
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    var value = ev.target.value;
                    var x = value.getInt16(0, true);
                    var y = value.getInt16(2, true);
                    var z = value.getInt16(4, true);
                    // �����x�v��x�����̒l��Unity�̃I�u�W�F�N�g�֒ʒm���܂�
                    // x�����̒l��micro:bit�̌X���Ƃ��ď������܂�
                    SendMessage(target, 'OnAccelerometerChanged', x);
                    SendMessage(target, 'OnAccelerometerChanged', y);
                    SendMessage(target, 'OnAccelerometerChanged', z);
                });

                // �{�^���T�[�r�X���擾���܂�
                return bluetoothServer.getPrimaryService(BUTTON_SERVICE_UUID);
            })
            .then(function (service) {
                console.log('getPrimaryService');

                buttonService = service;
                return buttonService.getCharacteristic(BUTTON_A_STATE_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // �{�^��A�̒ʒm�̎擾���J�n���܂�
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                // �{�^��A�̒ʒm���󂯎��܂�
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    var value = ev.target.value;
                    var state = value.getUint8();
                    // �{�^��A�̏�Ԃ�Unity�̃I�u�W�F�N�g�֒ʒm���܂�
                    SendMessage(target, 'OnButtonAChanged', state);
                });

                return buttonService.getCharacteristic(BUTTON_B_STATE_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // �{�^��B�̒ʒm�̎擾���J�n���܂�
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                // �{�^��B�̒ʒm���󂯎��܂�
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    var value = ev.target.value;
                    var state = value.getUint8();
                    // �{�^��B�̏�Ԃ�Unity�̃I�u�W�F�N�g�֒ʒm���܂�
                    SendMessage(target, 'OnButtonBChanged', state);
                });
            })
            .catch(function (err) {
                console.log('err:' + err);

                if (devices[target]) {
                    if (devices[target].gatt.connected) {
                        devices[target].gatt.disconnect();
                    }
                    delete devices[target];
                }
            });

        console.log('<<< connect');
    },

    // Bluetooth�f�o�C�X��ؒf���܂�
    Disconnect: function (targetName) {
        console.log('>>> disconnect');

        var target = Pointer_stringify(targetName);
        console.log('target:' + target);

        if (devices[target]) {
            console.log('device:' + devices[target]);
            // �f�o�C�X�ɐڑ����Ȃ�ؒf���܂�
            devices[target].gatt.disconnect();
            delete devices[target];
        }

        console.log('<<< disconnect');
    }
};
autoAddDeps(blePlugin, '$devices');
mergeInto(LibraryManager.library, blePlugin);

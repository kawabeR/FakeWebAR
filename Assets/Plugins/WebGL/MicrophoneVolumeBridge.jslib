mergeInto(LibraryManager.library, {

  MicrophoneVolumeBridge_RequestPermission: function (gameObjectNamePtr) {
    var goName = UTF8ToString(gameObjectNamePtr);
    if (window.__fakeARMicRequested) {
      return;
    }
    window.__fakeARMicRequested = true;
    window.__fakeARMicVolume = 0;
    window.__fakeARMicReady = false;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(function (stream) {
      var AudioContextClass = window.AudioContext || window.webkitAudioContext;
      var audioCtx = new AudioContextClass();
      var source = audioCtx.createMediaStreamSource(stream);
      var analyser = audioCtx.createAnalyser();
      analyser.fftSize = 512;
      source.connect(analyser);

      var dataArray = new Uint8Array(analyser.frequencyBinCount);
      function update() {
        analyser.getByteTimeDomainData(dataArray);
        var sumSquares = 0;
        for (var i = 0; i < dataArray.length; i++) {
          var v = (dataArray[i] - 128) / 128;
          sumSquares += v * v;
        }
        window.__fakeARMicVolume = Math.sqrt(sumSquares / dataArray.length);
        requestAnimationFrame(update);
      }
      update();

      window.__fakeARMicReady = true;
      SendMessage(goName, 'OnMicPermissionResult', '1');
    }).catch(function () {
      SendMessage(goName, 'OnMicPermissionResult', '0');
    });
  },

  MicrophoneVolumeBridge_GetVolume: function () {
    return window.__fakeARMicVolume || 0;
  },

  MicrophoneVolumeBridge_IsReady: function () {
    return window.__fakeARMicReady ? 1 : 0;
  }

});

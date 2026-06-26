window.neoVoiceInput = {
  connection: null,
  audioContext: null,
  processor: null,
  source: null,
  mediaStream: null,
  dotnetRef: null,
  hubUrl: '/hubs/voice-realtime',
  textareaId: '',
  isCapturing: false,
  targetSampleRate: 16000,
  baseText: '',
  sessionCommitted: '',
  partialText: '',

  init(dotNetRef, baseText, textareaId, hubUrl) {
    this.dotnetRef = dotNetRef;
    this.baseText = baseText || '';
    this.textareaId = textareaId || '';
    this.hubUrl = hubUrl || this.hubUrl;
    this.sessionCommitted = '';
    this.partialText = '';
  },

  getTextarea() {
    if (!this.textareaId) {
      return null;
    }

    return document.getElementById(this.textareaId);
  },

  async startRealtime() {
    if (!window.signalR) {
      throw new Error('SignalR 客户端未加载，请在 App.razor 中引入 @microsoft/signalr。');
    }

    if (!navigator.mediaDevices?.getUserMedia) {
      throw new Error('当前浏览器不支持麦克风录音。');
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .build();

    this.connection.on('RecognitionResult', (text, isFinal) => {
      this.applyRecognition(text, !!isFinal);
    });

    this.connection.on('RecognitionError', (message) => {
      if (this.dotnetRef) {
        this.dotnetRef.invokeMethodAsync('OnRecognitionError', message).catch(() => {});
      }
    });

    this.connection.on('SessionReady', () => {
      this.startPcmCapture().catch((error) => {
        if (this.dotnetRef) {
          this.dotnetRef.invokeMethodAsync('OnRecognitionError', error.message || String(error)).catch(() => {});
        }
      });
    });

    await this.connection.start();
    await this.connection.invoke('StartSession');
  },

  applyRecognition(text, isFinal) {
    if (isFinal) {
      this.sessionCommitted += text;
      this.partialText = '';
    } else {
      this.partialText = text;
    }

    const display = this.baseText + this.sessionCommitted + this.partialText;
    const textarea = this.getTextarea();
    if (textarea) {
      textarea.value = display;
    }

    if (this.dotnetRef) {
      this.dotnetRef.invokeMethodAsync('SyncDisplayText', display).catch(() => {});
    }
  },

  async stopRealtime() {
    this.isCapturing = false;

    if (this.connection) {
      try {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
          await this.connection.invoke('StopSession');
        }
      } finally {
        await this.connection.stop();
        this.connection = null;
      }
    }

    this.stopPcmCapture();
    return this.baseText + this.sessionCommitted + this.partialText;
  },

  async startPcmCapture() {
    if (this.isCapturing) {
      return;
    }

    this.mediaStream = await navigator.mediaDevices.getUserMedia({
      audio: {
        channelCount: 1,
        echoCancellation: true,
        noiseSuppression: true,
      },
    });

    this.audioContext = new AudioContext();
    if (this.audioContext.state === 'suspended') {
      await this.audioContext.resume();
    }

    const nativeSampleRate = this.audioContext.sampleRate;
    this.source = this.audioContext.createMediaStreamSource(this.mediaStream);

    const bufferSize = 4096;
    this.processor = this.audioContext.createScriptProcessor(bufferSize, 1, 1);
    this.isCapturing = true;

    this.processor.onaudioprocess = (event) => {
      if (!this.isCapturing || !this.connection) {
        return;
      }

      const input = event.inputBuffer.getChannelData(0);
      const pcmInput = nativeSampleRate === this.targetSampleRate
        ? input
        : this.downsample(input, nativeSampleRate, this.targetSampleRate);
      const pcm = this.floatTo16BitPCM(pcmInput);

      this.connection.invoke('SendAudio', Array.from(pcm)).catch(() => {});
    };

    this.source.connect(this.processor);
    this.processor.connect(this.audioContext.destination);
  },

  downsample(buffer, fromRate, toRate) {
    if (fromRate === toRate) {
      return buffer;
    }

    const ratio = fromRate / toRate;
    const newLength = Math.round(buffer.length / ratio);
    const result = new Float32Array(newLength);

    for (let i = 0; i < newLength; i += 1) {
      result[i] = buffer[Math.min(buffer.length - 1, Math.floor(i * ratio))];
    }

    return result;
  },

  stopPcmCapture() {
    this.isCapturing = false;

    if (this.processor) {
      this.processor.disconnect();
      this.processor.onaudioprocess = null;
      this.processor = null;
    }

    if (this.source) {
      this.source.disconnect();
      this.source = null;
    }

    if (this.audioContext) {
      this.audioContext.close().catch(() => {});
      this.audioContext = null;
    }

    if (this.mediaStream) {
      this.mediaStream.getTracks().forEach((track) => track.stop());
      this.mediaStream = null;
    }
  },

  floatTo16BitPCM(float32Array) {
    const buffer = new ArrayBuffer(float32Array.length * 2);
    const view = new DataView(buffer);

    for (let i = 0; i < float32Array.length; i += 1) {
      const sample = Math.max(-1, Math.min(1, float32Array[i]));
      view.setInt16(i * 2, sample < 0 ? sample * 0x8000 : sample * 0x7fff, true);
    }

    return new Uint8Array(buffer);
  },

  stopStream() {
    this.stopPcmCapture();
  },
};

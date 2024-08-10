class CallbackReceiver {
	#callbackId;
	#pullTimerId;
	#websocket;
	#answerBuffer;
	#noAnswersReported;
	#nonPendingAnswerReported;
	#onResultCallback;
	constructor(callbackId, wsPort) {
		this.#callbackId = callbackId;
		this.#answerBuffer = '';
		this.#noAnswersReported = 0;
		this.#nonPendingAnswerReported = false;
		this.#onResultCallback = null;
		this.#websocket = new WebSocket(`ws://127.0.0.1:${wsPort}/`);
		this.#websocket.onopen = (e) => {
			const that = this;
			this.#pullTimerId = setTimeout(function () { that.#websocketReadTimeout(); }, 1000);
		}
		this.#websocket.onclose = (e) => {
			this.close();
		}
		this.#websocket.onerror = (e) => {
			if (this.#onResult) {
				this.#processAnswerBuffer();
			} else {
				this.#onResult({ status: "error", output: e.data })
			}
			this.close();
		}
		this.#websocket.onmessage = (e) => {
			if (e.data && (typeof e.data === 'string' || e.data instanceof String) && e.data.length > 0) {
				this.#answerBuffer += e.data;
				this.#processAnswerBuffer();
				if (!this.#nonPendingAnswerReported) {
					const that = this;
					this.#pullTimerId = setTimeout(function () { that.#websocketReadTimeout(); }, 1000);
				} else {
					this.close();
				}
			} else if (e.data === null) {
				this.close();
			} else {
				if (this.#answerBuffer) {
					this.#processAnswerBuffer();
				}
				this.#onResult({ status: "error", output: `Unexpected (${ex}) non-json data received: ${e.data}` })
				this.close();
			}
		}
	}
	close() {
		if (this.#pullTimerId !== 0) {
			clearTimeout(this.#pullTimerId);
			this.#pullTimerId = 0;
		}
		if (this.#websocket !== null) {
			const ws = this.#websocket;
			this.#websocket = null;
			ws.close();
		}
		if (this.#answerBuffer) {
			this.#processAnswerBuffer();
			if (this.#answerBuffer) {
				this.#onResult({ status: "error", output: `Unprocessable data received: ${this.#answerBuffer}` })
			}
		}
		if (this.#noAnswersReported === 0) {
			this.#onResult({ status: "error", output: "No data received" })
		} else if (!this.#nonPendingAnswerReported) {
			this.#onResult({ status: "error", output: "No final data message received" })
		}
	}
	#websocketReadTimeout() {
		if (this.#websocket) {
			this.#websocket.send(`reqCallback:${this.#callbackId}`)
		}
	}
	#onResult(obj) {
		if (obj && this.#onResultCallback)
			this.#onResultCallback(obj);
	}
	setOnResultCallback(callback) {
		this.#onResultCallback = callback;
		if (this.#onResultCallback && this.#answerBuffer)
			this.#processAnswerBuffer();
	}
	#processAnswerBuffer() {
		let blockStart = 0;
		let blockEnd = 0;
		let inString = false;
		let escString = false;
		let blockCnt = 0;

		for (let i = 0; i < this.#answerBuffer.length; ++i) {
			switch (this.#answerBuffer[i]) {
				case '{':
					if (!inString) {
						if (blockCnt === 0) blockStart = i;
						blockCnt++;
					}
					break;
				case '}':
					if (!inString) {
						if (blockCnt > 0) blockCnt--;
						if (blockCnt === 0) {
							blockEnd = i;
							const message = this.#answerBuffer.substring(blockStart, blockEnd + 1);
							try {
								const resp = JSON.parse(message);
								if (this.#onResultCallback) {
									this.#noAnswersReported++;
									this.#onResult(resp);
								}
								if (resp.status && resp.status !== "unknown" && resp.status !== "pending") {
									this.#nonPendingAnswerReported = true;
								}
							}
							catch (ex) {
								this.#onResult({ status: "error", output: `Unexpected (${ex}) parsing received data: (${blockStart}..${blockEnd}) ${message}` });
							}
						}
					}
					break;
				case '"':
					if (!escString) {
						inString = !inString;
					}
					break;
			}
			escString = inString && (this.#answerBuffer[i] === '\\');
		}

		if (this.#onResultCallback) 
			this.#answerBuffer = this.#answerBuffer.substring(blockEnd + 1);
	}
};

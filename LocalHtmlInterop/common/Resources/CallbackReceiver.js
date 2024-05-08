class CallbackReceiver {
	#callbackId;
	#pullTimerId;
	#websocket;
	#answerCount;
	#result;
	#onResultCallback;
	constructor(callbackId, wsPort) {
		this.#callbackId = callbackId;
		this.#answerCount = 0;
		this.#result = null;
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
			if (this.#result === null) {
				this.#setResult({ status: "error", output: e.data })
			}
			this.close();
		}
		this.#websocket.onmessage = (e) => {
			try {
				const resp = JSON.parse(e.data);
				this.#answerCount++;
				if (((resp.status === "unknown") && (this.#answerCount < 10)) || (resp.status === "pending")) {
					const that = this;
					this.#pullTimerId = setTimeout(function () { that.#websocketReadTimeout(); }, 1000);
					return;
				}
				this.#setResult(resp);
			} catch (ex) {
				this.#setResult({ status: "error", output: "Unexpected non-json data received: " + e.data })
			}
			this.close();
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
		if (this.#result === null) {
			this.#setResult({ status: "error", output: "No data received" })
		}
	}
	#websocketReadTimeout() {
		if (this.#websocket) {
			this.#websocket.send(`reqCallback:${this.#callbackId}`)
		}
	}
	#setResult(obj) {
		this.#result = obj;
		if (this.#onResultCallback && this.#result)
			this.#onResultCallback(this.#result);
	}
	setOnResultCallback(callback) {
		this.#onResultCallback = callback;
		if (this.#onResultCallback && this.#result)
			this.#onResultCallback(this.#result);
	}
};

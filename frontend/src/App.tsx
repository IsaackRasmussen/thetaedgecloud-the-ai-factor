import { Route, Routes } from "react-router-dom";

import IndexPage from "@/pages/index";
import DocsPage from "@/pages/docs";
import PricingPage from "@/pages/pricing";
import BlogPage from "@/pages/blog";
import AboutPage from "@/pages/about";
//import MultiStreamsMixer from 'multistreamsmixer';
import RecordRTC, { invokeSaveAsDialog } from "recordrtc";

const WEBSOCKET_URL = "ws://127.0.0.1:8081/";

var pc: RTCPeerConnection, ws: WebSocket;

async function start(stream: MediaStream) {
  pc = new RTCPeerConnection();

  /*pc.ontrack = (evt) =>
    (document.querySelector("#videoCtl").srcObject = evt.streams[0]);*/

  pc.onicecandidate = (evt) =>
    evt.candidate && ws.send(JSON.stringify(evt.candidate));

  ws = new WebSocket(WEBSOCKET_URL, []);
  ws.onmessage = async function (evt) {
    var obj = JSON.parse(evt.data);
    if (obj?.candidate) {
      pc.addIceCandidate(obj);
    } else if (obj?.sdp) {
      await pc.setRemoteDescription(new RTCSessionDescription(obj));
      pc.createAnswer()
        .then((answer) => pc.setLocalDescription(answer))
        .then(() => ws.send(JSON.stringify(pc.localDescription)));
    }
  };
}

async function closePeer() {
  await pc?.close();
  await ws?.close();
}

navigator.mediaDevices
  .getUserMedia({
    video: true,
    audio: true,
  })
  .then(async function (stream) {
    await start(stream);

    /*let recorder = new RecordRTC(stream, {
      type: "video",
    });
    recorder.startRecording();
    recorder.toURL();


    const sleep = (m: any) => new Promise((r) => setTimeout(r, m));
    await sleep(3000);

    recorder.stopRecording(function () {
      let blob = recorder.getBlob();
      invokeSaveAsDialog(blob);
    });*/
  });

function App() {
  return (
    <Routes>
      <Route element={<IndexPage />} path="/" />
      <Route element={<DocsPage />} path="/docs" />
      <Route element={<PricingPage />} path="/pricing" />
      <Route element={<BlogPage />} path="/blog" />
      <Route element={<AboutPage />} path="/about" />
    </Routes>
  );
}

export default App;

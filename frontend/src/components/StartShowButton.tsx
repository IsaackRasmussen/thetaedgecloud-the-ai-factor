import { useState } from "react";
import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  useDisclosure,
} from "@nextui-org/react";
//import MultiStreamsMixer from 'multistreamsmixer';
import RecordRTC from "recordrtc";

var pc: RTCPeerConnection, ws: WebSocket;
var recorderRtc: RecordRTC | undefined = undefined;
var videoRef: any = undefined;

async function start() {
  let peerConn = new RTCPeerConnection();

  /*peerConn.ontrack = (evt) => {
    videoRef.srcObject = evt.streams[0];
  };*/

  peerConn.onicecandidate = (evt) =>
    evt.candidate && ws.send(JSON.stringify(evt.candidate));

  ws = new WebSocket(import.meta.env.VITE_WEBSOCKET_RTC_URL, []);
  ws.onmessage = async function (evt) {
    var obj = JSON.parse(evt.data);
    if (obj?.candidate) {
      peerConn.addIceCandidate(obj);
    } else if (obj?.sdp) {
      await peerConn.setRemoteDescription(new RTCSessionDescription(obj));
      peerConn
        .createAnswer()
        .then((answer) => peerConn.setLocalDescription(answer))
        .then(() => ws.send(JSON.stringify(peerConn.localDescription)));
    }
  };
}

async function closePeer() {
  await pc?.close();
  await ws?.close();
}

async function startShow() {
  navigator.mediaDevices
    .getUserMedia({
      video: true,
      audio: true,
    })
    .then(async function (stream) {
      console.log("test2");
      await start();

      recorderRtc = new RecordRTC(stream, {
        type: "video",
      });
      videoRef.srcObject = stream;
      recorderRtc.startRecording();
      recorderRtc.toURL();
    });
}

async function stopShow() {
  recorderRtc?.stopRecording(function () {
    closePeer();
  });
}

const StartShowButton = () => {
  const [isRecording, setIsRecording] = useState(false);
  const { isOpen, onOpen, onOpenChange } = useDisclosure();

  const handleRef = (video: any) => {
    videoRef = video;
  };

  return (
    <>
      <Button
        style={{ width: "10em", height: "5em", fontSize: "3em" }}
        radius="lg"
        disableRipple
        className="bg-gradient-to-tr from-pink-500 to-yellow-500 text-white shadow-lg relative overflow-visible rounded-full hover:-translate-y-1 px-12 shadow-xl bg-background/30 after:content-[''] after:absolute after:rounded-full after:inset-0 after:bg-background/40 after:z-[-1] after:transition after:!duration-500 hover:after:scale-150 hover:after:opacity-0"
        variant="bordered"
        onPress={onOpen}
      >
        Create show!
      </Button>

      <Modal
        isOpen={isOpen}
        placement="auto"
        onOpenChange={onOpenChange}
        size="5xl"
      >
        <ModalContent>
          {() => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                New Show
              </ModalHeader>
              <ModalBody>
                <video
                  id="videoCtl"
                  ref={handleRef}
                  controls
                  autoPlay
                  playsInline
                  style={{ width: "100%" }}
                ></video>{" "}
              </ModalBody>
              <ModalFooter>
                <Button
                  style={{ width: "10em", height: "2em", fontSize: "2em" }}
                  radius="lg"
                  disableRipple
                  className="shadow-lg relative overflow-visible rounded-full hover:-translate-y-1 px-12 shadow-xl bg-background/30 after:content-[''] after:absolute after:rounded-full after:inset-0 after:bg-background/40 after:z-[-1] after:transition after:!duration-500 hover:after:opacity-0"
                  variant="bordered"
                  color="danger"
                  onPress={() => {
                    if (isRecording) {
                      setIsRecording(false);
                      stopShow();
                    } else {
                      setIsRecording(true);
                      startShow();
                    }
                  }}
                >
                  {!isRecording ? "Start show!" : "Stop show"}
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};

export default StartShowButton;

import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@nextui-org/react";

const StreamPlayer = (props: any) => {
  return (
    <Modal
      isOpen={props.isOpen}
      onClose={props.onClose}
      size="5xl"
      style={{ height: "auto" }}
    >
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              {props.streamShow.name}
            </ModalHeader>
            <ModalBody>
              <iframe
                style={{
                  width: "100%",
                  height: "100%",
                  minHeight: "50vh",
                  backgroundColor: "white",
                }}
                src={props.streamShow.playerUri}
              />
            </ModalBody>
            <ModalFooter>
              <Button color="danger" onPress={onClose}>
                Close
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  );
};

export default StreamPlayer;

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
    <Modal isOpen={props.isOpen}>
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              {props.liveShow.name}
            </ModalHeader>
            <ModalBody>
              <iframe
                style={{ width: "100%", height: "100%" }}
                src={props.liveShow.playerUri}
              />
            </ModalBody>
            <ModalFooter>
              <Button color="danger" variant="light" onPress={onClose}>
                Close
              </Button>
              <Button color="primary" onPress={onClose}>
                Action
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  );
};

export default StreamPlayer;

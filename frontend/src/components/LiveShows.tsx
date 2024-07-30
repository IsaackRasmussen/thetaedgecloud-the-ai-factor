import useSWR from "swr";
import {
  Card,
  CardFooter,
  Image,
  Button,
  useDisclosure,
} from "@nextui-org/react";
import Lottie from "react-lottie-player";
import offlineJson from "../../lottiefiles/offline.json";

// Import useSWR from swr package

// created function to handle API request
const fetcher = (...args: any) => fetch(...args).then((res) => res.json());

const LiveShows = () => {
  const {
    data: liveShows,
    error,
    isValidating,
  } = useSWR(import.meta.env.VITE_API_URL + "/shows/live", fetcher, {
    refreshInterval: 1000,
  });

  // Handles error and loading state
  if (error) return <div className="failed">failed to load</div>;
  if (isValidating && !liveShows)
    return <div className="Loading">Loading...</div>;

  //const { isOpen, onOpen, onOpenChange } = useDisclosure();

  return (
    <div style={{ display: "flex", gap: "1em" }}>
      {liveShows &&
        liveShows.map((show: any, index: number) => (
          <Card
            isFooterBlurred
            radius="lg"
            className="border-none dark"
            key={index}
          >
            {show.status == "off" ? (
              <Lottie
                loop
                animationData={offlineJson}
                play
                style={{ width: 150, height: 150 }}
              />
            ) : (
              <Image
                alt="Streams"
                className="object-cover"
                height={200}
                src="/live_poster.jpg"
                width={200}
              />
            )}
            <CardFooter className="justify-between before:bg-white/10 border-white/20 border-1 overflow-hidden py-1 absolute before:rounded-xl rounded-large bottom-1 w-[calc(100%_-_8px)] shadow-small ml-1 z-10">
              <p className="text-tiny text-white/80">{show.name}</p>
              <Button
                className="text-tiny text-white bg-black/20"
                variant="flat"
                color="default"
                radius="lg"
                size="sm"
                disabled={show.status == "off"}
              >
                {show.status == "off" ? "Offline" : "Show"}
              </Button>
            </CardFooter>
          </Card>
        ))}
    </div>
  );
};

export default LiveShows;

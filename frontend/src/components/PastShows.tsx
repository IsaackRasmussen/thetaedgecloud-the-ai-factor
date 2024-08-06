import { useState } from "react";
import useSWR from "swr";
import {
  Table,
  TableHeader,
  TableColumn,
  TableBody,
  TableRow,
  TableCell,
  useDisclosure,
} from "@nextui-org/react";
import StreamPlayer from "./StreamPlayer";

// created function to handle API request
const fetcher = (...args: Parameters<typeof fetch>) =>
  fetch(...args).then((res) => res.json());

const PastShows = () => {
  const [currentShow, setCurrentShow] = useState(false);
  const {
    data: pastShows,
    error,
    isValidating,
  } = useSWR(import.meta.env.VITE_API_URL + "/shows/past", fetcher, {
    refreshInterval: 1000,
    revalidateIfStale: false,
    keepPreviousData: true,
  });
  const { isOpen, onOpen, onClose } = useDisclosure();

  // Handles error and loading state
  if (error) return <div className="failed">failed to load</div>;
  if (isValidating && !pastShows)
    return <div className="Loading">Loading...</div>;

  return (
    <div>
      <StreamPlayer
        isOpen={isOpen}
        onClose={onClose}
        streamShow={currentShow}
      />
      <Table
        selectionMode="single"
        fullWidth={true}
        defaultSelectedKeys={["2"]}
        aria-label="Previously recorded shows"
        onCellAction={(key: React.Key) => {
          const showIndex = parseInt(key.toString().split(".")[0]);
          setCurrentShow(pastShows[showIndex as number]);
          onOpen();
        }}
      >
        <TableHeader>
          <TableColumn>NAME</TableColumn>
          <TableColumn>Date</TableColumn>
        </TableHeader>
        <TableBody>
          {pastShows &&
            pastShows.map((show: any, index: number) => (
              <TableRow key={index}>
                <TableCell>{show.name}</TableCell>
                <TableCell>{show.date}</TableCell>
              </TableRow>
            ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default PastShows;

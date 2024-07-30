import React from "react";
import useSWR from "swr";
import {
  Table,
  TableHeader,
  TableColumn,
  TableBody,
  TableRow,
  TableCell,
  RadioGroup,
  Radio,
} from "@nextui-org/react";
import { DateInput } from "@nextui-org/react";
import { now, parseAbsoluteToLocal } from "@internationalized/date";
import { useDateFormatter } from "@react-aria/i18n";

// Import useSWR from swr package

// created function to handle API request
const fetcher = (...args: any) => fetch(...args).then((res) => res.json());
const colors = [
  "default",
  "primary",
  "secondary",
  "success",
  "warning",
  "danger",
];

const PastShows = () => {
  const [selectedColor, setSelectedColor] = React.useState("default");
  const {
    data: pastShows,
    error,
    isValidating,
  } = useSWR(import.meta.env.VITE_API_URL + "/shows/past", fetcher, {
    refreshInterval: 1000,
  });

  // Handles error and loading state
  if (error) return <div className="failed">failed to load</div>;
  if (isValidating && !pastShows)
    return <div className="Loading">Loading...</div>;

  return (
    <Table
      color={selectedColor}
      selectionMode="single"
      defaultSelectedKeys={["2"]}
      aria-label="Example static collection table"
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
              <TableCell>
                <DateInput
                  granularity="minute"
                  label="Date and time"
                  value={parseAbsoluteToLocal(show.date)}
                />
              </TableCell>
            </TableRow>
          ))}
      </TableBody>
    </Table>
  );
};

export default PastShows;

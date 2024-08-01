import DefaultLayout from "@/layouts/default";
import LiveShows from "@/components/LiveShows";
import PastShows from "@/components/PastShows";
import StartShowButton from "@/components/StartShowButton";

export default function IndexPage() {
  return (
    <DefaultLayout>
      <section className="flex flex-col items-center justify-center gap-16 py-8 md:py-10 dark">
        <div
          className="inline-block max-w-lg text-center justify-center"
          style={{ width: "100%" }}
        >
          <LiveShows />
        </div>

        <div>
          <StartShowButton />
        </div>

        <div>
          <PastShows />
        </div>
      </section>
    </DefaultLayout>
  );
}

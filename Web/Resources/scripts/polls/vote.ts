import alpine from "alpinejs";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";

//#region Alpine
document.addEventListener("alpine:init", () => {
    alpine.data('vote', () => ({
        init() {
            // Only not set up a SignalR connection if the poll has not concluded.
            // @ts-ignore
            if (ongoing === true) {
                this.connection = new HubConnectionBuilder().withUrl("/web").build();
                this.connection.on("AcknowledgeSubscription", poll => console.log(`Successfully subscribed to updates for poll #${poll}`));
                this.connection.on("AcknowledgeVote", (option: number) => this.chosen = option);
                this.connection.on("AcknowledgeRetractVote", () => setTimeout(() => this.chosen = null, 1000));
                this.connection.on("UpdateResults", (results: number[]) => this.results = results);
                this.connection.start().then(() => this.connection.invoke("Subscribe", this.pollId));
            }
        },
        
        connection: null as HubConnection | null,
        viewResults: false,
        // @ts-ignore
        pollId: pollId as number,
        // @ts-ignore
        chosen: chosen as number | null,
        // @ts-ignore
        results: initialResults as number[],
        
        vote(optionId: number) {
            // @ts-ignore
            this.connection.invoke("Vote", this.pollId, optionId);
        },
        
        retract() {
            // @ts-ignore
            this.connection.invoke("RetractVote", this.pollId, this.chosen);
        }
    }));
});

// Start all the things
alpine.start();
//#endregion
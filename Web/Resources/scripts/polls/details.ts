import alpine from 'alpinejs';
import { Chart, registerables } from "chart.js";
import { camelize, camelizeKeys, pascalize } from 'humps';
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";

//#region Interfaces
interface PollResult {
    optionId: number;
    votes: number;
}

interface ChannelResult {
    results: PollResult[];
    live: boolean;
    lastUpdated: string;
}

interface ChannelPoll {
    id: number,
    channel: number,
    identifier: string,
}

interface Option {
    id: number,
    text: string,
}

interface Poll {
    id: number,
    question: string,
    startedAt: string,
    endedAt: string | null,
    creator: null,
    channelPolls: ChannelPoll[],
    options: Option[]
    status: number
}

interface ChannelTotal {
    channel: string;
    votes: number;
}

interface OptionTotal {
    option: string;
    votes: number;
}

interface TopChannels {
    channels: string;
    suffix: string;
}

interface TopOptions {
    options: string;
    suffix: string;
}

interface Results {
    [index: string]: ChannelResult;
}
//#endregion

//#region Initializers
const optionsChartElement = <HTMLCanvasElement> document.getElementById('optionsChart');
const channelsChartElement = <HTMLCanvasElement> document.getElementById('channelsChart');

const optionsChartCtx = optionsChartElement.getContext('2d');
const channelsChartCtx = channelsChartElement.getContext('2d');

let optionsChart: Chart;
let channelsChart: Chart;

Chart.register(...registerables);
//#endregion

//#region Alpine
document.addEventListener("alpine:init", () => {
    alpine.data('details', () => ({
        init() {
            initCharts(this.optionTotals, this.channelTotals);
            this.$watch("results", _ => {
                updateCharts(this.optionTotals.map((t: OptionTotal) => t.votes), this.channelTotals.map((c: ChannelTotal) => c.votes));
            })

            this.connection = new HubConnectionBuilder().withUrl("/tally").build();

            this.connection.on("AcknowledgeSubscription", poll => console.log(`Successfully subscribed to updates for poll #${poll}`));
            this.connection.on("UpdateResults", (results: Results)  => {
                this.results = new Map<string, ChannelResult>(Object.entries(camelizeKeys(results)));
                this.refreshing = false;
                this.showMessage("Success!", "Successfully refreshed results.", "success");
            });
            this.connection.on("UpdateResult", (channel: string, result: ChannelResult) => {
                this.results.set(camelize(channel), result);
                
                // Alpine is not calling the $watch handler. This is a workaround until I figure that out.
                updateCharts(this.optionTotals.map((t: OptionTotal) => t.votes), this.channelTotals.map((c: ChannelTotal) => c.votes));
            });
            this.connection.start().then(() => this.connection.invoke("Subscribe", this.poll.id));
        },
        // @ts-ignore
        poll: initialPoll as Poll,
        // @ts-ignore
        results: new Map<string, ChannelResult>(Object.entries(initialResults)),
        connection: null as HubConnection | null,
        message: null,
        refreshing: false,

        get channelTotals() : ChannelTotal[] {
            return [...this.results]
                .map(([key, value]) => ({
                    channel: pascalize(key),
                    votes: value.results.reduce((accumulator, value) => accumulator + value.votes, 0)
                }))
                .sort((a, b) => a.votes > b.votes ? -1 : a.votes < b.votes ? 1 : 0);
        },
        get topChannels() : TopChannels {
            const firstChannel = this.channelTotals[0];
            if (firstChannel.votes !== this.channelTotals[1].votes)
            {
                // There's only one top channel
                return { channels: firstChannel.channel, suffix: "was the most active channel." }
            }

            // Multiple top channels
            const topChannels = this.channelTotals
                .filter(c => c.votes === firstChannel.votes)
                .map(c => c.channel);

            const lastIndex = topChannels.length - 1;
            return {
                channels: `${topChannels.slice(0, lastIndex).join(", ")} and ${topChannels[lastIndex]}`,
                suffix: "were the most active channels.",
            };
        },

        get optionTotals() : OptionTotal[] {
            const optionTotalsDict = new Map<string, number>();
            const allResults = [...this.results].map(([, v]) => v.results).reduce((acc, value) => acc.concat(value), []);

            for (const {optionId, votes} of allResults) {
                if (optionTotalsDict.has(optionId))
                    optionTotalsDict.set(optionId, optionTotalsDict.get(optionId) + votes);
                else
                    optionTotalsDict.set(optionId, votes);
            }

            return [...optionTotalsDict]
                .map(([key, value]) => ({
                    option: this.poll.options.find(o => o.id === parseInt(key)).text,
                    votes: value
                }));
        },
        get orderedOptionTotals() : OptionTotal[] {
            return this.optionTotals.sort((a, b) => a.votes > b.votes ? -1 : a.votes < b.votes ? 1 : 0);
        },
        get topOptions() : TopOptions {
            const firstOption = this.orderedOptionTotals[0];
            if (firstOption.votes !== this.orderedOptionTotals[1].votes)
            {
                // There's only one top option
                return { options: firstOption.option, suffix: "was the most popular option." }
            }

            // Multiple top options
            const topOptions = this.orderedOptionTotals
                .filter(c => c.votes === firstOption.votes)
                .map(c => c.option);

            const lastIndex = topOptions.length - 1;
            return {
                options: `${topOptions.slice(0, lastIndex).join(", ")} and ${topOptions[lastIndex]}`,
                suffix: "were the most popular options.",
            };
        },

        get totalVotes() : number {
            return this.channelTotals.reduce((acc, value) => acc + value.votes, 0);
        },

        refresh() : void {
            this.connection.invoke("Refresh", this.poll.id)
        },
        showMessage(title: string, body: string, severity: string) {
            this.message = { title, body, severity }
            setTimeout(() => this.message = null, 3000);
        }
    }));
});

// Start all the things
alpine.start();
//#endregion

//#region Charts
function initCharts(optionTotals: OptionTotal[], channelTotals: ChannelTotal[]) {
    optionsChart = new Chart(optionsChartCtx, {
        type: 'bar',
        data: {
            labels: optionTotals.map(t => t.option),
            datasets: [{
                label: '# of votes per option',
                data: optionTotals.map(t => t.votes),
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)'
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                }
            }
        }
    });

    channelsChart = new Chart(channelsChartCtx, {
        type: 'doughnut',
        data: {
            labels: channelTotals.map(t => t.channel),
            datasets: [{
                label: '# of votes per channel',
                data: channelTotals.map(t => t.votes),
                backgroundColor: [
                    'rgb(255, 99, 132)',
                    'rgb(54, 162, 235)'
                ],
                hoverOffset: 4
            }]
        },
    });
}

function updateCharts(optionsData: number[], channelsData: number[]) {    
    optionsChart.data.datasets[0].data = optionsData;
    channelsChart.data.datasets[0].data = channelsData;

    optionsChart.update();
    channelsChart.update();
}
//#endregion
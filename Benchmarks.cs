using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Net.Http;

//call it in program.cs
//ApiParallelBenchmarks api = new ApiParallelBenchmarks();
//var a = await api.SemaphoreVersion();

//encountered windows defender or anti virus errors
//var config = DefaultConfig.Instance
//    .AddJob(Job
//         .MediumRun
//         .WithLaunchCount(1)
//         .WithToolchain(InProcessNoEmitToolchain.Instance));
//BenchmarkRunner.Run<ApiParallelBenchmarks>();
//return;

namespace ConsoleApp1.Benchmark
{
    public class ApiParallelBenchmarks
    {
        private static readonly HttpClient HttpClient = new();
        private const int TaskCount = 100;

        [Benchmark]
        public async Task<List<int>> ForEachVersion()
        {
            var tasks = new List<int>();
            var youtubeSubscriibersTasks = Enumerable.Range(0, TaskCount)
                .Select( _ => new Func<Task<int>>(() => GetYoutubeSubscribersAsync(HttpClient))).ToList();
            foreach (var youtubeSunscribersTask in youtubeSubscriibersTasks)
            {
                tasks.Add(await youtubeSunscribersTask());
            }
            return tasks;
        }

        public List<int> ParallelVersion(int maxDegreeOfParallelism)
        {
            var list = new List<int>();
            var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<int>(() => GetYoutubeSubscribersAsync(HttpClient).GetAwaiter().GetResult())).ToList();
            Parallel.For(0, youtubeSubscribersTasks.Count, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, i => list.Add(youtubeSubscribersTasks[i]()));
            return list;
        }

        [Benchmark]
        public List<int> UnlimitedParallelVersion() => ParallelVersion(-1); // -1: detect the available cpu cores

        [Benchmark]
        public List<int> LimitedParallelVersion() => ParallelVersion(4);
        //{
        //    var list = new List<int>();
        //    var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
        //        .Select(_ => new Func<int>(() => GetYoutubeSubscribersAsync(HttpClient).GetAwaiter().GetResult())).ToList();
        //    Parallel.For(0, youtubeSubscribersTasks.Count, i => list.Add(youtubeSubscribersTasks[i]()));
        //    return list;
        //}

        [Benchmark]
        public async Task<List<int>> WhenAllVersion()
        {
            var tasks = Enumerable.Range(0, TaskCount)
                .Select(_ => GetYoutubeSubscribersAsync(HttpClient));
            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        [Benchmark]
        public async Task<List<int>> AsyncParallelVersion1() => await AsyncParallelVersion(1);

        [Benchmark]
        public async Task<List<int>> AsyncParallelVersion10() => await AsyncParallelVersion(10);

        //[Benchmark]
        //public async Task<List<int>> AsyncParallelVersion100() => await AsyncParallelVersion(100);

        public static Task ParallelForEachAsync<T>(IEnumerable<T> source, int degreeOfParallelism, Func<T, Task> body)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition) 
                {
                    while (partition.MoveNext())
                        await body(partition.Current);
                }
            }

            return Task.WhenAll(
                Partitioner.Create(source)
                .GetPartitions(degreeOfParallelism)
                .AsParallel()
                .Select(AwaitPartition));
        }

        public async Task<List<int>> AsyncParallelVersion(int batches)
        {
            var list = new List<int>();
            var tasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<Task<int>>(() => GetYoutubeSubscribersAsync(HttpClient)))
                .ToList();
            await ParallelForEachAsync(tasks, batches, async func =>
            {
                list.Add(await func());
            });
            return list;
        }

        private static async Task<int> GetYoutubeSubscribersAsync(HttpClient httpClient)
        {
            var response = await httpClient.GetStringAsync($"http://localhost:500/users");
            var youtubeUser = JsonSerializer.Deserialize<YoutubeUser>(response);
            return youtubeUser!.Subscribers;
        }

        [Benchmark]
        public async Task<List<string>> SemaphoreVersion()
        {
            var throttler = new SemaphoreSlim(10, 100);
            //var tasks = Enumerable.Range(0, TaskCount)
            //    .Select(i => DoSomethingAsync(i));
            
            var tasks = Enumerable.Range(0, TaskCount)
                .Select(async i =>
                {
                    await throttler.WaitAsync();

                    try
                    {
                        return await DoSomethingAsync(i);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                })
                .ToList();

            var result = await Task.WhenAll(tasks);
            return result.ToList();
        }

        //same as GetYoutubeSubscribersAsync
        private static async Task<string> DoSomethingAsync(int i)
        {
            var response = await HttpClient.GetStringAsync($"http://localhost:5000/users");
            //var youtubeUser = JsonSerializer.Deserialize<YoutubeUser>(response);
            //return youtubeUser!.Subscribers;
            Console.WriteLine(response);
            return response;
        }
    }

    public class YoutubeUser
    {
        public int Id { get; set; }
        public int Subscribers { get; set; }
    }
}

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Benchmark
{
    [MemoryDiagnoser(false)]
    public class Benchmarks
    {
        private readonly OpenClass _openClass = new();
        private readonly SealedClass _sealedClass = new();
        private readonly OpenClass[] _openClasses = new OpenClass[1];
        private readonly SealedClass[] _sealedClasses = new SealedClass[1];

        [Params(100, 100_000, 1_000_000)]
        public int Size { get; set; }
        public int[] _items;
        public List<int> _itemsList;
        public List<Wrapper> _wrapperItemsList;
        public Wrapper[] _wrapperArray;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(420);
            _items = Enumerable.Range(0, 100).Select(x => random.Next()).ToArray();
            _wrapperItemsList = Enumerable.Range(0, 100).Select(x =>
            {
                var number = random.Next();
                return new Wrapper(number, number.ToString());
            }).ToList();
            _wrapperArray = Enumerable.Range(0, 100).Select(x =>
            {
                var number = random.Next();
                return new Wrapper(number, number.ToString());
            }).ToArray();
        }

        [Benchmark]
        public int[] For()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                DoSomething(item);
            }

            return _items;
        }

        [Benchmark]
        public void LoopSpanList1()
        {
            Span<int> listAsSpan = CollectionsMarshal.AsSpan(_itemsList);
            for (var i = 0; i < listAsSpan.Length; i++)
            {
                var item = listAsSpan[i];
                DoSomething(item);
            }
        }

        [Benchmark]
        public void LoopSpanList2()
        {
            //Span<int> asSpan = _items;
            Span<int> listAsSpan = CollectionsMarshal.AsSpan(_itemsList);
            ref var searchSpace = ref MemoryMarshal.GetReference(listAsSpan);

            for (var i = 0; i < listAsSpan.Length; i++)
            {
                var item = Unsafe.Add(ref searchSpace, i);
                DoSomething(item);
            }
        }

        [Benchmark]
        public void LoopSpanList3()
        {
            //Span<Wrapper> wrapperListAsSpan = CollectionsMarshal.AsSpan(_wrapperItemsList);
            
            //or
            //Span<Wrapper> wrapperListAsSpan = _wrapperArray;
            //ref var searchSpace = ref MemoryMarshal.GetReference(wrapperListAsSpan);
            //for (var i = 0; i < wrapperListAsSpan.Length; i++)
            //{
            //    var item = Unsafe.Add(ref searchSpace, i);
            //    DoSomething(item.Number);
            //}

            ref var searchSpace = ref MemoryMarshal.GetArrayDataReference(_wrapperArray);
            for (var i = 0; i < _wrapperArray.Length; i++)
            {
                var item = Unsafe.Add(ref searchSpace, i);
                DoSomething(item.Number);
            }
        }

        public record Wrapper(int Number, string Text);

        [Benchmark]
        public int[] ForSpan()
        {
            Span<int> asSpan = _items;
            for (var i = 0; i < asSpan.Length; i++)
            {
                var item = asSpan[i];
                DoSomething(item);
            }

            return _items;
        }

        [Benchmark]
        public int[] Foreach()
        {
            foreach (var item in _items)
            {
                DoSomething(item);
            }

            return _items;
        }

        [Benchmark]
        public int[] ForeachSpan()
        {
            Span<int> asSpan = _items;
            for (var i = 0; i < asSpan.Length; i++)
            {
                var item = asSpan[i];
                DoSomething(item);
            }

            return _items;
        }

        void DoSomething(int i) 
        { 
            Console.WriteLine(i);
        }

        [Benchmark]
        public void OpenVoid() => _openClass.VoidMethod();

        [Benchmark]
        public void SelaedVoid() => _sealedClass.VoidMethod();

        [Benchmark]
        public int OpenInt() => _openClass.IntMethod() + 2;

        [Benchmark]
        public int SealedInt() => _sealedClass.IntMethod() + 2;

        [Benchmark]
        public bool IsCheckOpen() => _openClass is OpenClass;

        [Benchmark]
        public bool IsCheckSealed() => _sealedClass is SealedClass;

        [Benchmark]
        public Span<OpenClass> SpanOpen() => _openClasses;

        [Benchmark]
        public Span<SealedClass> SpanSealed() => _sealedClasses;
    }

    public class IntWrap
    {
        public int Value { get; }

        public IntWrap(int number)
        {
            Value = number;
        }

        public static implicit operator int(IntWrap num) => num.Value;
        public static implicit operator IntWrap(int num) => new IntWrap(num);
    }

    public class OpenClass 
    { 
        public void VoidMethod() { }
        public int IntMethod() => 1;
    }
    public sealed class SealedClass 
    {
        public void VoidMethod() { }
        public int IntMethod() => 1;
    }
}

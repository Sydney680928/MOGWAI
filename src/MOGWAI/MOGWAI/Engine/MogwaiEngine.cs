// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MOGWAI.Interfaces;
using MOGWAI.Objects;
using MOGWAI.Primitives;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace MOGWAI.Engine

{
    public sealed class MogwaiEngine
    {
        private static readonly char[] _invalidCharsExtended = [' ', '\'', '{', '}', '«', '»', '(', ')', '[', ']', '"', ':', '\r', '\n', '\t'];
        private static readonly char[] _invalidChars =  [' ', '\'', '!', '{', '}', '«', '»', '(', ')', '[', ']', '"', ':', '\r', '\n', '\t'];

        private string _name;
        private int _debugPort;
        private List<Stack<MOGObject>> _stacks = [];
        private Stack<MOGObject> _currentStack;
        private IDelegate? _delegate;

        private FrozenDictionary<string, MOGPrimitive> _primitivesByName;
        private Dictionary<string, MOGPrimitive> _initializingPrimitivesByName = [];

        private FrozenDictionary<Type, MOGPrimitive> _primitivesByType;
        private Dictionary<Type, MOGPrimitive> _initializingPrimitivesByType = [];

        private string[] _hostFunctions = [];
        private Parser _parser = new();
        private readonly List<VarContext> _varsContext = [];
        private VarContext? _currentLocalVarsContext;
        private List<bool> _breakRequested = new();
        private MOGFunction? _lastProgram;
        private int _lastParsingHash;
        private Dictionary<string, MOGFunction> _functions = [];
        private Dictionary<string, MOGTimer> _timers = [];
        private Dictionary<string, MOGEvent> _events = [];
        private Queue<MOGFireObject> _fireObjectsQueue = [];
        private object _fireObjectsQueueLock = new();
        private bool _debugMode;
        private object _debugNextStepSignalLock = new();
        private object _debugResumeSignalLock = new();
        private SocketServerService _socketServerService;
        private DatagramManager _datagramManager;
        private Random _random = new Random();
        private Dictionary<Type, MOGType> _types = [];
        private Dictionary<string, MOGType> _typesByName = [];
        private bool _keepAlive;
        private bool _disableInterrupts;
        private MOGType _typeAny;
        private int _tronValue;
        private Dictionary<string, bool> _flags = new();
        private Dictionary<string, MOGTask> _tasks = new();
        private SemaphoreSlim _fireEventSemaphore = new(1);
        private Dictionary<string, FileStream> _openinFiles = [];
        private Dictionary<string, FileStream> _openoutFiles = [];
        private Dictionary<int, MogwaiExecutionContext> _includes = [];
        private Dictionary<string, PluginInformations> _plugins = [];
        private List<string> _varsInAutoEval = new();

        // MOX Signature = [STX][M ][O ][G ][W ][A ][I ][28][09][19][68][ETX]
        //               = 00   01  02  03  04  05  06  07  08  09  10  11
        //               = [02 ][4D][4F][47][57][41][49][1C][09][13][44][03 ]
        private byte[] _MOXSign = [0x02, 0x4D, 0x4F, 0x47, 0x57, 0x41, 0x49, 0x1C, 0x09, 0x13, 0x44, 0x03];

        #region CTOR

        public MogwaiEngine(string name) : this(name, false, true)
        {

        }

        public MogwaiEngine(string name, bool useDefaultFolders) : this(name, false, useDefaultFolders)
        {

        }

        public MogwaiEngine(string name, bool keepAlive, bool useDefaultFolders)
        {
            _name = name;
            _keepAlive = keepAlive;
            _typeAny = new(this, "any");

            TaskResult = new MOGNull(this);

            // Create main stack

            _stacks.Add(new());
            _currentStack = _stacks[0];

            // Create SocketServer and DatagramManager

            _socketServerService = new SocketServerService();
            _datagramManager = new DatagramManager();

            var r = new Random();
            _debugPort = r.Next(63000, 65000);

            // Create vars context
            // Context zéro = Global vars

            _varsContext.Add(new VarContext("GLOBAL"));          

            #region DEFINE TYPES

            RegisterType(typeof(MOGString), "string");
            RegisterType(typeof(MOGNumber), "number");
            RegisterType(typeof(MOGName), "name");
            RegisterType(typeof(MOGList), "list");
            RegisterType(typeof(MOGRecord), "record");
            RegisterType(typeof(MOGCode), "code");
            RegisterType(typeof(MOGFunction), "function");
            RegisterType(typeof(MOGPrimitive), "primitive");
            RegisterType(typeof(MOGWord), "word");
            RegisterType(typeof(MOGKey), "key");
            RegisterType(typeof(MOGBoolean), "boolean");
            RegisterType(typeof(MOGData), "data");
            RegisterType(typeof(MOGBinaryNumber), "binary");
            RegisterType(typeof(MOGNull), "null");
            RegisterType(typeof(MOGObject), "any");
            RegisterType(typeof(MOGEmpty), "any");
            RegisterType(typeof(MOGType), "type");
            RegisterType(typeof(MOGRef), "ref");
            RegisterType(typeof(MOGVar), "var");
            RegisterType(typeof(MOGHostFunction), "hfunc");

            #endregion

            #region DEFINE PRIMITIVES

            // Maths functions

            RegisterPublicPrimitive(new PrimitiveGeneralPlus(this, "+"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathSubstract(this, "-"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathMultiply(this, "*"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathDevide(this, "/"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathNegate(this, "+/-"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathSin(this, "sin"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathCos(this, "cos"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathTan(this, "tan"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathAsin(this, "asin"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathAcos(this, "acos"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathAtan(this, "atan"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathPI(this, "PI"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathAbs(this, "abs"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathSqrt(this, "sqrt"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathFloor(this, "floor"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathCeil(this, "ceil"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathPow(this, "pow"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathModulo(this, "mod"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathAverage(this, "average"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveMathSum(this, "sum"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveBinaryComplement(this, "~"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveBinaryAnd(this, "&"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveBinaryOr(this, "|"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveBinaryXor(this, "^"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveRightShift(this, ">>"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveLeftShift(this, "<<"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveBinaryUp(this, "up"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveBinaryDown(this, "down"), MOGPrimitive.CATEGORY_MATHS);
            RegisterPublicPrimitive(new PrimitiveTestBitFromBinaryNumber(this, "bit?"), MOGPrimitive.CATEGORY_MATHS);

            // Vars functions

            RegisterPublicPrimitive(new PrimitiveIncr(this, "++"));
            RegisterPublicPrimitive(new PrimitiveDecr(this, "--"));
            RegisterPublicPrimitive(new PrimitiveRcl(this, "rcl"));
            RegisterPublicPrimitive(new PrimitiveRclx(this, "rclx"));
            RegisterPublicPrimitive(new PrimitiveExists(this, "exists"));
            RegisterPublicPrimitive(new PrimitiveFuncs(this, "funcs"));
            RegisterPublicPrimitive(new PrimitiveGetGlobalVars(this, "vars"));
            RegisterPublicPrimitive(new PrimitiveGetLocalVars(this, "lvars"));
            RegisterPublicPrimitive(new PrimitiveBAG(this, "bag"));

            // Conversion Function

            RegisterPublicPrimitive(new PrimitiveConvertToInt(this, "->int"));
            RegisterPublicPrimitive(new PrimitiveGetType(this, "->type"));
            RegisterPublicPrimitive(new PrimitiveStackToVars(this, "->vars"));
            RegisterPublicPrimitive(new PrimitiveStackToSafeVars(this, "->safeVars"));
            RegisterPublicPrimitive(new PrimitiveStackToParams(this, "->params"));
            RegisterPublicPrimitive(new PrimitiveLower(this, "->lower"));
            RegisterPublicPrimitive(new PrimitiveUpper(this, "->upper"));
            RegisterPublicPrimitive(new PrimitiveToNumber(this, "->num"));
            RegisterPublicPrimitive(new PrimitiveToHex(this, "->hex"));
            RegisterPublicPrimitive(new PrimitiveFromHexToNumber(this, "hex->"));
            RegisterPublicPrimitive(new PrimitiveToString(this, "->str"));
            RegisterPublicPrimitive(new PrimitiveToUTF8(this, "->utf8"));
            RegisterPublicPrimitive(new PrimitiveUTF8ToString(this, "utf8->"));
            RegisterPublicPrimitive(new PrimitiveToBase64(this, "->base64"));
            RegisterPublicPrimitive(new PrimitiveFromBase64(this, "base64->"));
            RegisterPublicPrimitive(new PrimitiveFromASCII7(this, "ascii7->"));
            RegisterPublicPrimitive(new PrimitiveToASCII7(this, "->ascii7"));
            RegisterPublicPrimitive(new PrimitiveFromASCII8(this, "ascii->"));
            RegisterPublicPrimitive(new PrimitiveToASCII8(this, "->ascii"));
            RegisterPublicPrimitive(new PrimitiveToMD5(this, "->md5"));
            RegisterPublicPrimitive(new PrimitiveToSHA1(this, "->sha1"));
            RegisterPublicPrimitive(new PrimitiveToSHA256(this, "->sha256"));
            RegisterPublicPrimitive(new PrimitiveToSHA512(this, "->sha512"));
            RegisterPublicPrimitive(new PrimitiveToList(this, "->list"));
            RegisterPublicPrimitive(new PrimitiveToFormat(this, "->format"));
            RegisterPublicPrimitive(new PrimitiveToName(this, "->name"));
            RegisterPublicPrimitive(new PrimitiveToKey(this, "->key"));
            RegisterPublicPrimitive(new PrimitiveToData(this, "->data"));
            RegisterPublicPrimitive(new PrimitiveToFunction(this, "->function"));
            RegisterPublicPrimitive(new PrimitiveToCode(this, "->code"));
            RegisterPublicPrimitive(new PrimitiveToPrimitive(this, "->primitive"));
            RegisterPublicPrimitive(new PrimitiveMathToDeg(this, "->deg"));
            RegisterPublicPrimitive(new PrimitiveMathToRad(this, "->rad"));
            RegisterPublicPrimitive(new PrimitiveToUri(this, "->uri"));
            RegisterPublicPrimitive(new PrimitiveToUrlEncode(this, "->urlEncode"));
            RegisterPublicPrimitive(new PrimitiveCompress(this, "->compress"));
            RegisterPublicPrimitive(new PrimitiveDecompress(this, "->decompress"));
            RegisterPublicPrimitive(new PrimitivePack(this, "->pack"));
            RegisterPublicPrimitive(new PrimitiveUnpack(this, "->unpack"));
            RegisterPublicPrimitive(new PrimitiveObjectToJson(this, "->json"));
            RegisterPublicPrimitive(new PrimitiveJsonToObject(this, "json->"));
            RegisterPublicPrimitive(new PrimitiveEscape(this, "->escape"));
            RegisterPublicPrimitive(new PrimitiveUnescape(this, "->unescape"));
            RegisterPublicPrimitive(new PrimitiveToChar(this, "->char"));
            RegisterPublicPrimitive(new PrimitiveFromChar(this, "char->"));

            RegisterPublicPrimitive(new PrimitiveToUInt8(this, "->u8"));
            RegisterPublicPrimitive(new PrimitiveToUInt16(this, "->u16"));
            RegisterPublicPrimitive(new PrimitiveToUInt32(this, "->u32"));
            RegisterPublicPrimitive(new PrimitiveToUInt64(this, "->u64"));
            RegisterPublicPrimitive(new PrimitiveToInt8(this, "->i8"));
            RegisterPublicPrimitive(new PrimitiveToInt16(this, "->i16"));
            RegisterPublicPrimitive(new PrimitiveToInt32(this, "->i32"));
            RegisterPublicPrimitive(new PrimitiveToInt64(this, "->i64"));

            RegisterPublicPrimitive(new PrimitiveToBinaryNumber(this, "->bin"));
            RegisterPublicPrimitive(new PrimitiveToBinaryNumber8(this, "->bin8"));
            RegisterPublicPrimitive(new PrimitiveToBinaryNumber16(this, "->bin16"));
            RegisterPublicPrimitive(new PrimitiveToBinaryNumber24(this, "->bin24"));
            RegisterPublicPrimitive(new PrimitiveToBinaryNumber32(this, "->bin32"));
            RegisterPublicPrimitive(new PrimitiveToBinaryNumber48(this, "->bin48"));
            RegisterPublicPrimitive(new PrimitiveToBinaryNumber64(this, "->bin64"));

            RegisterPublicPrimitive(new PrimitiveToDataLE8(this, "->dataLE8"));
            RegisterPublicPrimitive(new PrimitiveToDataLE16(this, "->dataLE16"));
            RegisterPublicPrimitive(new PrimitiveToDataLE24(this, "->dataLE24"));
            RegisterPublicPrimitive(new PrimitiveToDataLE32(this, "->dataLE32"));
            RegisterPublicPrimitive(new PrimitiveToDataLE48(this, "->dataLE48"));
            RegisterPublicPrimitive(new PrimitiveToDataLE64(this, "->dataLE64"));

            RegisterPublicPrimitive(new PrimitiveToDataBE8(this, "->dataBE8"));
            RegisterPublicPrimitive(new PrimitiveToDataBE16(this, "->dataBE16"));
            RegisterPublicPrimitive(new PrimitiveToDataBE24(this, "->dataBE24"));
            RegisterPublicPrimitive(new PrimitiveToDataBE32(this, "->dataBE32"));
            RegisterPublicPrimitive(new PrimitiveToDataBE48(this, "->dataBE48"));
            RegisterPublicPrimitive(new PrimitiveToDataBE64(this, "->dataBE64"));

            RegisterPublicPrimitive(new PrimitiveToDataBE(this, "->dataBE"));
            RegisterPublicPrimitive(new PrimitiveToDataLE(this, "->dataLE"));

            RegisterPublicPrimitive(new PrimitiveToNumberLE8(this, "dataLE8->"));
            RegisterPublicPrimitive(new PrimitiveToNumberLE16(this, "dataLE16->"));
            RegisterPublicPrimitive(new PrimitiveToNumberLE24(this, "dataLE24->"));
            RegisterPublicPrimitive(new PrimitiveToNumberLE32(this, "dataLE32->"));
            RegisterPublicPrimitive(new PrimitiveToNumberLE48(this, "dataLE48->"));
            RegisterPublicPrimitive(new PrimitiveToNumberLE64(this, "dataLE64->"));

            RegisterPublicPrimitive(new PrimitiveToNumberBE8(this, "dataBE8->"));
            RegisterPublicPrimitive(new PrimitiveToNumberBE16(this, "dataBE16->"));
            RegisterPublicPrimitive(new PrimitiveToNumberBE24(this, "dataBE24->"));
            RegisterPublicPrimitive(new PrimitiveToNumberBE32(this, "dataBE32->"));
            RegisterPublicPrimitive(new PrimitiveToNumberBE48(this, "dataBE48->"));
            RegisterPublicPrimitive(new PrimitiveToNumberBE64(this, "dataBE64->"));

            RegisterPublicPrimitive(new PrimitiveToNumberLE(this, "dataLE->"));
            RegisterPublicPrimitive(new PrimitiveToNumberBE(this, "dataBE->"));

            RegisterPublicPrimitive(new PrimitiveToFloatLE32(this, "dataLE32F->"));
            RegisterPublicPrimitive(new PrimitiveToFloatBE32(this, "dataBE32F->"));

            RegisterPublicPrimitive(new PrimitiveToFloatLE64(this, "dataLE64F->"));
            RegisterPublicPrimitive(new PrimitiveToFloatBE64(this, "dataBE64F->"));

            RegisterPublicPrimitive(new PrimitiveToDataFromFloatLE32(this, "->dataLE32F"));
            RegisterPublicPrimitive(new PrimitiveToDataFromFloatBE32(this, "->dataBE32F"));

            RegisterPublicPrimitive(new PrimitiveToDataFromFloatLE64(this, "->dataLE64F"));
            RegisterPublicPrimitive(new PrimitiveToDataFromFloatBE64(this, "->dataBE64F"));

            // Output screen functions

            RegisterPublicPrimitive(new PrimitivePrintLn(this, "?"));
            RegisterPublicPrimitive(new PrimitivePrintLn(this, "console.println"));
            RegisterPublicPrimitive(new PrimitivePrint(this, "??"));
            RegisterPublicPrimitive(new PrimitivePrint(this, "console.print"));
            RegisterPublicPrimitive(new PrimitiveDump(this, "?d"));
            RegisterPublicPrimitive(new PrimitiveClearScreen(this, "console.clear"));
            RegisterPublicPrimitive(new PrimitiveConsoleInput(this, "console.input"));
            RegisterPublicPrimitive(new PrimitiveConsolePrompt(this, "console.prompt"));
            RegisterPublicPrimitive(new PrimitiveConsoleShow(this, "console.show"));
            RegisterPublicPrimitive(new PrimitiveConsoleHide(this, "console.hide"));
            RegisterPublicPrimitive(new PrimitiveConsoleLocate(this, "console.locate"));
            RegisterPublicPrimitive(new PrimitiveConsoleGetCursorPosition(this, "console.cursor"));
            RegisterPublicPrimitive(new PrimitiveConsoleSetForegroundColor(this, "console.setForegroundColor"));
            RegisterPublicPrimitive(new PrimitiveConsoleSetBackgroundColor(this, "console.setBackgroundColor"));
            RegisterPublicPrimitive(new PrimitiveConsoleGetInputKey(this, "console.getInputKey"));

            // General functions

            RegisterPublicPrimitive(new PrimitivePurge(this, "purge"));
            RegisterPublicPrimitive(new PrimitiveGet(this, "get"));
            RegisterPublicPrimitive(new PrimitiveSet(this, "set"));
            RegisterPublicPrimitive(new PrimitiveKeys(this, "keys"));
            RegisterPublicPrimitive(new PrimitiveSize(this, "size"));
            RegisterPublicPrimitive(new PrimitiveLeft(this, "left"));
            RegisterPublicPrimitive(new PrimitiveRight(this, "right"));
            RegisterPublicPrimitive(new PrimitiveContains(this, "contains"));
            RegisterPublicPrimitive(new PrimitiveSub(this, "sub"));
            RegisterPublicPrimitive(new PrimitiveFirst(this, "first"));
            RegisterPublicPrimitive(new PrimitiveLast(this, "last"));
            RegisterPublicPrimitive(new PrimitiveIsNull(this, "isnull"));
            RegisterPublicPrimitive(new PrimitiveIsEmpty(this, "isEmpty"));
            RegisterPublicPrimitive(new PrimitiveSplit(this, "split"));
            RegisterPublicPrimitive(new PrimitiveJoin(this, "join"));
            RegisterPublicPrimitive(new PrimitiveWhere(this, "where"));
            RegisterPublicPrimitive(new PrimitiveCreateGUID(this, "guid"));
            RegisterPublicPrimitive(new PrimitiveCreateUnique(this, "unique"));
            RegisterPublicPrimitive(new PrimitiveMathRand(this, "rand"));
            RegisterPublicPrimitive(new PrimitiveExtract(this, "extract"));
            RegisterPublicPrimitive(new PrimitiveButFirst(this, "butfirst"));
            RegisterPublicPrimitive(new PrimitiveButLast(this, "butlast"));
            RegisterPublicPrimitive(new PrimitiveMin(this, "min"));
            RegisterPublicPrimitive(new PrimitiveMax(this, "max"));
            RegisterPublicPrimitive(new PrimitiveEnvMachineName(this, "env.machineName"));
            RegisterPublicPrimitive(new PrimitiveProcessStart(this, "process.start"));

            // Stack functions

            RegisterPublicPrimitive(new PrimitiveStackClear(this, "clear"), MOGPrimitive.CATEGORY_STACK);
            RegisterPublicPrimitive(new PrimitiveStackSize(this, "depth"), MOGPrimitive.CATEGORY_STACK);
            RegisterPublicPrimitive(new PrimitiveStackDup(this, "dup"), MOGPrimitive.CATEGORY_STACK);
            RegisterPublicPrimitive(new PrimitiveStackSwap(this, "swap"), MOGPrimitive.CATEGORY_STACK);
            RegisterPublicPrimitive(new PrimitiveStackDrop(this, "drop"), MOGPrimitive.CATEGORY_STACK);
            RegisterPublicPrimitive(new PrimitiveStackSign(this, "sign"), MOGPrimitive.CATEGORY_STACK);
            RegisterPublicPrimitive(new PrimitiveStackCheck(this, "check"), MOGPrimitive.CATEGORY_STACK);

            // Control functions

            RegisterPublicPrimitive(new PrimitiveEval(this, "eval"));
            RegisterPublicPrimitive(new PrimitiveWait(this, "wait"));
            RegisterPublicPrimitive(new PrimitiveBreak(this, "break"));
            RegisterPublicPrimitive(new PrimitiveReturn(this, "return"));

            // Runtime functions

            RegisterPublicPrimitive(new PrimitiveMogwaiHalt(this, "mogwai.halt"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiExit(this, "mogwai.exit"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiReset(this, "mogwai.reset"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiInfo(this, "mogwai.info"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveSendMessageToHost(this, "mogwai.sendMessage"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiIsTask(this, "mogwai.isTask"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveClearCaches(this, "mogwai.cclear"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiUsing(this, "mogwai.using"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiUsings(this, "mogwai.usings"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveDI(this, "DI"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveEI(this, "EI"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiInclude(this, "mogwai.include"), MOGPrimitive.CATEGORY_RUNTIME);
            RegisterPublicPrimitive(new PrimitiveMogwaiStrict(this, "mogwai.strict"), MOGPrimitive.CATEGORY_RUNTIME);

            // Compare functions

            RegisterPublicPrimitive(new PrimitiveEqual(this, "=="));
            RegisterPublicPrimitive(new PrimitiveNotEqual(this, "!="));
            RegisterPublicPrimitive(new PrimitiveIsSuperior(this, ">"));
            RegisterPublicPrimitive(new PrimitiveIsSuperiorOrEqual(this, ">="));
            RegisterPublicPrimitive(new PrimitiveIsLess(this, "<"));
            RegisterPublicPrimitive(new PrimitiveIsLessOrEqual(this, "<="));
            RegisterPublicPrimitive(new PrimitiveLike(this, "like"));
            RegisterPublicPrimitive(new PrimitiveConditionalAnd(this, "and"));
            RegisterPublicPrimitive(new PrimitiveConditionalOr(this, "or"));
            RegisterPublicPrimitive(new PrimitiveConditionalXor(this, "xor"));
            RegisterPublicPrimitive(new PrimitiveConditionalNot(this, "not"));

            // Timer functions

            RegisterPublicPrimitive(new PrimitiveTimerStart(this, "timer.start"));
            RegisterPublicPrimitive(new PrimitiveTimerStop(this, "timer.stop"));
            RegisterPublicPrimitive(new PrimitiveTimerPurge(this, "timer.purge"));
            RegisterPublicPrimitive(new PrimitiveTimerState(this, "timer.state"));
            RegisterPublicPrimitive(new PrimitiveTimerList(this, "timer.list"));

            // Flag functions

            RegisterPublicPrimitive(new PrimitiveFlagSet(this, "flag.set"));
            RegisterPublicPrimitive(new PrimitiveFlagClear(this, "flag.clear"));
            RegisterPublicPrimitive(new PrimitiveFlagIsSet(this, "flag.isSet"));
            RegisterPublicPrimitive(new PrimitiveFlagIsClear(this, "flag.isClear"));

            // Debug functions

            RegisterPublicPrimitive(new PrimitiveDebugHalt(this, "¤"), MOGPrimitive.CATEGORY_DEBUG);
            RegisterPublicPrimitive(new PrimitiveDebugHalt(this, "debug.halt"), MOGPrimitive.CATEGORY_DEBUG);
            RegisterPublicPrimitive(new PrimitiveDebugTron(this, "debug.tron"), MOGPrimitive.CATEGORY_DEBUG);
            RegisterPublicPrimitive(new PrimitiveDebugTroff(this, "debug.troff"), MOGPrimitive.CATEGORY_DEBUG);
            RegisterPublicPrimitive(new PrimitiveDebugWrite(this, "debug.write"), MOGPrimitive.CATEGORY_DEBUG);
            RegisterPublicPrimitive(new PrimitiveDebugClear(this, "debug.clear"), MOGPrimitive.CATEGORY_DEBUG);

            // Error functions

            RegisterPublicPrimitive(new PrimitiveErrorLast(this, "error.last"), MOGPrimitive.CATEGORY_ERROR);
            RegisterPublicPrimitive(new PrimitiveErrorReset(this, "error.reset"), MOGPrimitive.CATEGORY_ERROR);
            RegisterPublicPrimitive(new PrimitiveErrorThrow(this, "error.throw"), MOGPrimitive.CATEGORY_ERROR);
            RegisterPrivatePrimitive(new PrimitiveErrorTrap(this, "TRAP"), "trap");
            RegisterPrivatePrimitive(new PrimitiveErrorGuard(this, "GUARD"), "guard...else");

            // Task functions

            RegisterPrivatePrimitive(new PrimitiveTASKDEF(this, "TASK.DEF"), "task def");
            RegisterPrivatePrimitive(new PrimitiveTASKSTART(this, "TASK.START"), "task...start");
            RegisterPrivatePrimitive(new PrimitiveTASKSEND(this, "TASK.SEND"), "task...send");
            RegisterPublicPrimitive(new PrimitiveTaskIsRunning(this, "task.isRunning"));
            RegisterPublicPrimitive(new PrimitiveTaskStop(this, "task.stop"));
            RegisterPublicPrimitive(new PrimitiveTaskPurge(this, "task.purge"));
            RegisterPublicPrimitive(new PrimitiveTaskPublish(this, "task.publish"));
            RegisterPublicPrimitive(new PrimitiveTaskSetResult(this, "task.setResult"));
            RegisterPublicPrimitive(new PrimitiveTaskGetResult(this, "task.result"));
            RegisterPublicPrimitive(new PrimitiveTaskWait(this, "task.wait"));
            RegisterPublicPrimitive(new PrimitiveTaskJoin(this, "task.join"));
            RegisterPublicPrimitive(new PrimitiveTaskGetName(this, "task.name"));
            RegisterPublicPrimitive(new PrimitiveTaskList(this, "task.list"));

            // DateTime functions

            RegisterPublicPrimitive(new PrimitiveNow(this, "now"));
            RegisterPublicPrimitive(new PrimitiveToDate(this, "->date"));
            RegisterPublicPrimitive(new PrimitiveFromDate(this, "date->"));
            RegisterPublicPrimitive(new PrimitiveToDuration(this, "->duration"));
            RegisterPublicPrimitive(new PrimitiveFromDuration(this, "duration->"));
            RegisterPublicPrimitive(new PrimitiveToDurations(this, "->durations"));

            // Event functions

            RegisterPrivatePrimitive(new PrimitiveEVENT(this, "EVENT"), "onEvent");
            RegisterPublicPrimitive(new PrimitiveEventFire(this, "event.fire"));
            RegisterPublicPrimitive(new PrimitiveEventPurge(this, "event.purge"));
            RegisterPublicPrimitive(new PrimitiveEventList(this, "event.list"));

            // Path functions

            RegisterPublicPrimitive(new PrimitivePathMake(this, "path.make"));
            RegisterPublicPrimitive(new PrimitivePathDesktop(this, "path.desktop"));
            RegisterPublicPrimitive(new PrimitivePathMyDocuments(this, "path.documents"));
            RegisterPublicPrimitive(new PrimitivePathMyMusic(this, "path.music"));
            RegisterPublicPrimitive(new PrimitivePathMyVideos(this, "path.videos"));
            RegisterPublicPrimitive(new PrimitivePathMyPictures(this, "path.pictures"));
            RegisterPublicPrimitive(new PrimitivePathProgramData(this, "path.programData"));
            RegisterPublicPrimitive(new PrimitivePathTempDirectory(this, "path.tempDirectory"));
            RegisterPublicPrimitive(new PrimitivePathTempFilename(this, "path.tempFilename"));
            RegisterPublicPrimitive(new PrimitivePathProgramsDirectory(this, "path.programs"));
            RegisterPublicPrimitive(new PrimitivePathFilesDirectory(this, "path.files"));
            RegisterPublicPrimitive(new PrimitivePathUsingsDirectory(this, "path.usings"));
            RegisterPublicPrimitive(new PrimitivePathSetUsingsDirectory(this, "path.setUsings"));
            RegisterPublicPrimitive(new PrimitivePathSetProgramsDirectory(this, "path.setPrograms"));
            RegisterPublicPrimitive(new PrimitivePathSetFilesDirectory(this, "path.setFiles"));

            // Directory functions

            RegisterPublicPrimitive(new PrimitiveDirectoryGetDirectories(this, "dir.directories"));
            RegisterPublicPrimitive(new PrimitiveDirectoryGetFiles(this, "dir.files"));
            RegisterPublicPrimitive(new PrimitiveDirectoryCreate(this, "dir.create"));
            RegisterPublicPrimitive(new PrimitiveDirectoryPurge(this, "dir.purge"));
            RegisterPublicPrimitive(new PrimitiveDirectoryExist(this, "dir.exists"));
            RegisterPublicPrimitive(new PrimitiveDirectoryRename(this, "dir.rename"));
            RegisterPublicPrimitive(new PrimitiveDirectorySetCurrent(this, "dir.setCurrent"));
            RegisterPublicPrimitive(new PrimitiveDirectoryGetCurrent(this, "dir.current"));

            // File functions

            RegisterPublicPrimitive(new PrimitiveFilePurge(this, "file.purge"));
            RegisterPublicPrimitive(new PrimitiveFileRename(this, "file.rename"));
            RegisterPublicPrimitive(new PrimitiveFileCopy(this, "file.copy"));
            RegisterPublicPrimitive(new PrimitiveFileDataRead(this, "file.data.read"));
            RegisterPublicPrimitive(new PrimitiveFileDataWrite(this, "file.data.write"));
            RegisterPublicPrimitive(new PrimitiveFileOpenIn(this, "file.open"));
            RegisterPublicPrimitive(new PrimitiveFileOpenOut(this, "file.create"));
            RegisterPublicPrimitive(new PrimitiveFileAppend(this, "file.append"));
            RegisterPublicPrimitive(new PrimitiveFileRead(this, "file.read"));
            RegisterPublicPrimitive(new PrimitiveFileReadLine(this, "file.readLine"));
            RegisterPublicPrimitive(new PrimitiveFileWrite(this, "file.write"));
            RegisterPublicPrimitive(new PrimitiveFileClose(this, "file.close"));
            RegisterPublicPrimitive(new PrimitiveFileSize(this, "file.size"));
            RegisterPublicPrimitive(new PrimitiveFileEof(this, "file.eof"));
            RegisterPublicPrimitive(new PrimitiveFileFileInfo(this, "file.info"));
            RegisterPublicPrimitive(new PrimitiveFileExist(this, "file.exists"));

            // Internet functions

            RegisterPublicPrimitive(new PrimitiveHttpGet(this, "http.get"));
            RegisterPublicPrimitive(new PrimitiveHttpPost(this, "http.post"));

            // Private primitivesreturn Task.FromResult(EvalResult.NoError);

            RegisterPrivatePrimitive(new PrimitiveSTO(this, "STO"), "->");
            RegisterPrivatePrimitive(new PrimitiveDEFUNC(this, "DEFUNC"), "to");
            RegisterPrivatePrimitive(new PrimitiveIF(this, "IF"), "if");
            RegisterPrivatePrimitive(new PrimitiveIFELSE(this, "IFELSE"), "if...else");
            RegisterPrivatePrimitive(new PrimitiveFOREACH(this, "FOREACH"), "foreach");
            RegisterPrivatePrimitive(new PrimitiveFOREACHTRANSFORM(this, "FOREACHTRANSFORM"), "foreach");
            RegisterPrivatePrimitive(new PrimitiveFOR(this, "FOR"), "for");
            RegisterPrivatePrimitive(new PrimitiveFORSTEP(this, "FORSTEP"), "for");
            RegisterPrivatePrimitive(new PrimitiveREPEAT(this, "REPEAT"), "repeat");
            RegisterPrivatePrimitive(new PrimitiveWHILE(this, "WHILE"), "while");
            RegisterPrivatePrimitive(new PrimitiveDOWHILE(this, "DOWHILE"), "do...while");
            RegisterPrivatePrimitive(new PrimitiveDURING(this, "DURING"), "during");
            RegisterPrivatePrimitive(new PrimitiveFOREVER(this, "FOREVER"), "forever do");
            RegisterPrivatePrimitive(new PrimitiveEVERY(this, "EVERY"), "timer...every");
            RegisterPrivatePrimitive(new PrimitiveAFTER(this, "AFTER"), "timer...after");
            RegisterPrivatePrimitive(new PrimitiveLATER(this, "LATER"), "after");
            RegisterPrivatePrimitive(new PrimitiveSTOPLUS(this, "STO+"), "->+");
            RegisterPrivatePrimitive(new PrimitiveSTOSUBSTRACT(this, "STO-"), "->-");
            RegisterPrivatePrimitive(new PrimitiveSTOMULTIPLY(this, "STO*"), "->*");
            RegisterPrivatePrimitive(new PrimitiveSTODIVIDE(this, "STO/"), "->/");
            RegisterPrivatePrimitive(new PrimitiveSWITCH(this, "SWITCH"), "switch");
            RegisterPrivatePrimitive(new PrimitiveDECLARE(this, "DECLARE"), "=>");
            RegisterPrivatePrimitive(new PrimitivePIPEREF(this, "PIPEREF"), "-->");

            _primitivesByName = _initializingPrimitivesByName.ToFrozenDictionary();
            _initializingPrimitivesByName.Clear();

            _primitivesByType = _initializingPrimitivesByType.ToFrozenDictionary(); 
            _initializingPrimitivesByType.Clear();

            #endregion

            if (useDefaultFolders)
            {
                var personnalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                var rootPath = Path.Combine(personnalPath, "MOGWAI");
                ProgramsDirectory = Path.Combine(rootPath, "Programs");
                UsingsDirectory = Path.Combine(rootPath, "Usings");
                FilesDirectory = Path.Combine(rootPath, "Files");

                try
                {
                    // On crée les dossiers par défaut de MOGWAI

                    Directory.CreateDirectory(rootPath);
                    Directory.CreateDirectory(ProgramsDirectory);
                    Directory.CreateDirectory(UsingsDirectory);
                    Directory.CreateDirectory(FilesDirectory);
                }
                catch
                {

                }
            }
            else
            {
                UsingsDirectory = Directory.GetCurrentDirectory();
                ProgramsDirectory = Directory.GetCurrentDirectory();
                FilesDirectory = Directory.GetCurrentDirectory();
            }
        }

        #endregion

        #region ENUMS

        public enum PlatformsEnum
        {
            Unknown,
            Windows,
            Linux,
            FreeBSD,
            OSX,
            Android,
            iOS
        }

        public enum CodeOrigin
        {
            Unknown,
            Mog,
            Mox
        }

        #endregion

        #region PROPERTIES

        public static Version RuntimeVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                string strVersion = attr?.Version ?? string.Empty;
                return new Version(strVersion);
            }
        }

        public static string RuntimePrompt
        {
            get
            {
                var year = DateTime.Now.Year < 2026 ? 2026 : DateTime.Now.Year;
                return $"MOGWAI version {RuntimeVersion?.ToString() ?? "?.?.?"}\r\n(c) Stéphane SIBUE 2015-{year}";
            }
        }

        public string Name => _name;

        public int StackSize => _currentStack.Count;

        public IDelegate? Delegate
        {
            get => _delegate;

            set
            {
                _delegate = value;
                _hostFunctions = _delegate?.HostFunctions(this) ?? [];  
            }
        }

        public bool AllowPrivatePrimitives { get; set; } = false;

        public MOGPrimitive? GetPrimitive(string name, bool duplicate)
        {
            if (_primitivesByName.TryGetValue(name, out var primitive))
            {
                if (duplicate)
                    return primitive.Duplicate();

                return primitive;
            }

            return null;
        }

        public MOGPrimitive? GetPrimitive(Type type, bool duplicate)
        {
            if (_primitivesByType.TryGetValue(type, out var primitive))
            {
                if (duplicate)
                    return primitive.Duplicate();
                
                return primitive;
            }

            return null;
        }

        public bool BreakRequested
        {
            get
            {
                if (_breakRequested.Count == 0)
                    return false;

                return _breakRequested[0];
            }
        }

        public bool HaltRequested { get; set; }

        public bool ExitRequested { get; set; }

        public bool ReturnRequested { get; set; }

        public bool IsRunning { get; private set; }

        public int TronValue
        {
            get => _tronValue;

            set => _tronValue = value > 0 ? value : 0;
        }

        public Error LastError { get; set; } = Error.None;

        public List<string> Primitives
        {
            get
            {
                if (AllowPrivatePrimitives)
                {
                    return AllPrimitives;
                }
                else
                {
                    return PublicPrimitives;
                }
            }
        }

        private List<string> PrivatePrimitives
        {
            get
            {
                var list = new List<string>();

                foreach (var key in _primitivesByName.Keys)
                {
                    if (_primitivesByName[key].IsPrivate)
                        list.Add(key);
                }

                return list;
            }
        }

        private List<string> PublicPrimitives
        {
            get
            {
                var list = new List<string>();

                foreach (var key in _primitivesByName.Keys)
                {
                    if (!_primitivesByName[key].IsPrivate)
                        list.Add(key);
                }

                return list;
            }
        }

        private List<string> AllPrimitives => _primitivesByName.Keys.ToList();

        public bool KeepAlive => _keepAlive;

        public string UsingsDirectory { get; set; }

        public string ProgramsDirectory { get; set; }

        public string FilesDirectory { get; set; }

        internal string? TaskName { get; init; }

        internal bool IsTask => MotherEngine != null;

        internal MogwaiEngine? MotherEngine { get; init; }

        internal MOGObject TaskResult { get; set; }

        internal bool AllowExtendedNames { get; set; } = false;

        internal PluginInformations[] PluginInformations => _plugins.Values.ToArray();

        internal MOGObject? CurrentEvalObject { get; set; }

        internal bool StrictMode { get; set; } = false;

        public bool IsPaused => DebugPauseMode;

        public bool IsSocketServerRunning => _socketServerService != null && _socketServerService.IsRunning;

        internal string[] HostFunctions => Delegate?.HostFunctions(this) ?? [];

        internal int LastParserStartErrorPosition { get; set; } = -1;

        internal int LastParserEndErrorPosition { get; set; } = -1;

        internal MogwaiExecutionContext? LastParserExecutionContext { get; set; }

        internal bool IsSocketServerServiceRunning => _socketServerService != null && _socketServerService.IsRunning;

        #endregion

        #region PRIVATE FUNCTIONS   

        private void RegisterPublicPrimitive(MOGPrimitive primitive, string category = MOGPrimitive.CATEGORY_GENERAL)
        {
            primitive.IsPrivate = false;
            primitive.Category = category;

            _initializingPrimitivesByName[primitive.Name] = primitive;

            var type = primitive.GetType();

            if (!_initializingPrimitivesByType.ContainsKey(type))
                _initializingPrimitivesByType[type] = primitive;
        }

        private void RegisterPrivatePrimitive(MOGPrimitive primitive, string friendlyName)
        {
            primitive.IsPrivate = true;
            primitive.FriendlyName = friendlyName;
           
            _initializingPrimitivesByName[primitive.Name] = primitive;

            var type = primitive.GetType();

            if (!_initializingPrimitivesByType.ContainsKey(type))
                _initializingPrimitivesByType[type] = primitive;
        }

        private void RegisterType(Type type, string name)
        {
            var tp = new MOGType(this, name, 0);
            _types[type] = tp;
            _typesByName[name] = tp;
        }

        internal bool TypeExists(string name) => _typesByName.ContainsKey(name);
        
        internal async Task<EvalResult> ExecuteAsync(string code, bool debugMode)
        {
            try
            {
                await Reset(_keepAlive);

                _debugMode = debugMode;

                if (string.IsNullOrEmpty(code))
                    return EvalResult.NoError;

                var stopwatch = Stopwatch.StartNew();

                if (MotherEngine != null && TaskName != null)
                    await MotherEngine.FireEvent(MOGTask.EVENT_TASK_DID_START, new MOGName(MotherEngine, TaskName, 0));

                if (Delegate != null)
                    await Delegate.ProgramStart(this, code);

                await SendProgramStart(debugMode);

                MOGFunction program;

                var hash = code.GetHashCode();

                if (_lastProgram != null && _lastParsingHash == hash)
                {
                    program = _lastProgram;
                }
                else
                {
                    try
                    {
                        program = new MOGFunction(this, code, 0, null);

                        _lastProgram = program;
                        _lastParsingHash = hash;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();

                        var r = EvalResult.ParseFailure(this, ex.Message);

                        if (IsTask)
                        {
                            var failureInformations = new MOGRecord(MotherEngine!);
                            failureInformations.Items["task"] = new MOGName(MotherEngine!, TaskName!, 0);
                            failureInformations.Items["error"] = new MOGString(MotherEngine!, r.Error.Code, 0);
                            failureInformations.Items["message"] = new MOGString(MotherEngine!, r.Error.Message, 0);

                            await MotherEngine!.FireEvent(MOGTask.EVENT_TASK_DID_FAIL, failureInformations);
                        }

                        if (Delegate != null)
                            await Delegate.ProgramEnd(this, r);

                        await SendProgramEndWithError(r);

                        return r;
                    }
                }

                HaltRequested = false;
                ExitRequested = false;
                ReturnRequested = false;

                EvalResult result = await program.Execute();

                HaltRequested = false;
                ExitRequested = false;
                ReturnRequested = false;

                EvalResult result2;

                if (result.IsError)
                {
                    if (_functions.TryGetValue("MOGWAI.onError", out var onErrorFunction))
                    {
                        result2 = await onErrorFunction.Execute();

                        if (result2.IsError)
                            result = result2;
                    }
                }
                else
                {
                    if (_functions.TryGetValue("MOGWAI.onStop", out var onStopFunction))
                    {
                        result2 = await onStopFunction.Execute();

                        if (result2.IsError)
                            result = result2;
                    }
                }

                stopwatch.Stop();
                result!.Duration = stopwatch.Elapsed;

                await Reset(_keepAlive);

                _debugMode = false;

                if (Delegate != null)
                    await Delegate.ProgramEnd(this, result!);

                if (result != EvalResult.NoError)
                {
                    if (CurrentEvalObject != null)
                    {
                        if (CurrentEvalObject.StartPos == -1 || CurrentEvalObject.EndPos == -1)
                        {
                            result.StartErrorPosition = LastParserStartErrorPosition;
                            result.EndErrorPosition = LastParserEndErrorPosition;
                        }
                        else
                        {
                            result.StartErrorPosition = CurrentEvalObject.StartPos;
                            result.EndErrorPosition = CurrentEvalObject.EndPos;
                        }
                    }
                    else
                    {
                        result.StartErrorPosition = LastParserStartErrorPosition;
                        result.EndErrorPosition = LastParserEndErrorPosition;
                    }

                    if (MotherEngine != null && TaskName != null)
                    {
                        var failureInformations = new MOGRecord(MotherEngine!);
                        failureInformations.Items["task"] = new MOGName(MotherEngine, TaskName, 0);
                        failureInformations.Items["error"] = new MOGString(MotherEngine, result.Error.Code, 0);
                        failureInformations.Items["message"] = new MOGString(MotherEngine, result.Error.Message, 0);

                        await MotherEngine.FireEvent(MOGTask.EVENT_TASK_DID_FAIL, failureInformations);
                    }

                    await SendProgramEndWithError(result);
                }
                else
                {
                    if (MotherEngine != null && TaskName != null)
                    {
                        var endInformations = new MOGRecord(MotherEngine);
                        endInformations.Items["task"] = new MOGName(MotherEngine, TaskName, 0);
                        endInformations.Items["result"] = TaskResult;

                        await MotherEngine.FireEvent(MOGTask.EVENT_TASK_DID_END, endInformations);
                    }

                    await SendProgramEndWithoutError(result);
                }

                return result;
            }
            finally
            {
                IsRunning = false;
            }
        }

        private static PlatformsEnum GetRuntimePlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return PlatformsEnum.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return PlatformsEnum.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return PlatformsEnum.FreeBSD;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return PlatformsEnum.Windows;
            }
            else
            {
                if (RuntimeInformation.RuntimeIdentifier.Contains("ANDROID", StringComparison.InvariantCultureIgnoreCase))
                {
                    return PlatformsEnum.Android;
                }
                else if (RuntimeInformation.RuntimeIdentifier.Contains("IOS", StringComparison.InvariantCultureIgnoreCase))
                {
                    return PlatformsEnum.iOS;
                }
            }

            return PlatformsEnum.Unknown;
        }

        private static string GetPlatformName(PlatformsEnum platform)
        {
            switch (platform)
            {
                case PlatformsEnum.OSX:
                    return "OSX";

                case PlatformsEnum.Unknown:
                    return "?";

                case PlatformsEnum.Windows:
                    return "WINDOWS";

                case PlatformsEnum.Android:
                    return "ANDROID";

                case PlatformsEnum.FreeBSD:
                    return "FREEBSD";

                case PlatformsEnum.iOS:
                    return "IOS";

                case PlatformsEnum.Linux:
                    return "LINUX";

                default:
                    return "?";
            }
        }

        internal Dictionary<string, string> GetVersionInformations()
        {
            var dico = new Dictionary<string, string>();
            var version = RuntimeVersion;

            string platform = GetPlatformName(GetRuntimePlatform());

            dico.Add("mogwai", version.ToString());
            dico.Add("platform", platform);
            dico.Add("architecture", RuntimeInformation.OSArchitecture.ToString());
            dico.Add("OSdescription", RuntimeInformation.OSDescription);
            dico.Add("framework", RuntimeInformation.FrameworkDescription);
            dico.Add("runtimeIdentifier", RuntimeInformation.RuntimeIdentifier);
            dico.Add("prompt", $"MOGWAI RUNTIME {version}\n© 2016-{DateTime.Now.Year} Stéphane SIBUE");

            return dico;
        }

        private List<string> GetPublicPrimitiveNames()
        {
            var lst = new List<string>();

            foreach (var p in _primitivesByName.Values)
            {
                if (!p.IsPrivate)
                    lst.Add(p.Name);
            }

            return lst;
        }

        private List<string> GetPrivatePrimitiveNames()
        {
            var lst = new List<string>();

            foreach (var p in _primitivesByName.Values)
            {
                if (p.IsPrivate)
                    lst.Add(p.Name);
            }

            return lst;
        }

        internal string ObjectVisualization(MOGObject obj)
        {
            var sb = new StringBuilder();

            if (obj is MOGList list)
            {
                if (list.Size > 0)
                {
                    var f = "{0:" + new string('0', (int)Math.Ceiling(Math.Log10(list.Size))) + "} : ";

                    for (int i = 0; i < list.Size; i++)
                    {
                        sb.Append(string.Format(f, i));
                        sb.AppendLine(Tools.BeginOfString(list.GetItem(i)?.ToString() ?? "", 50));
                    }
                }
            }
            else if (obj is MOGRecord record)
            {
                int keymax = 10;
                foreach (var key in record.Items.Keys)
                    if (key.Length > keymax) keymax = key.Length;

                foreach (var key in record.Items.Keys)
                {
                    sb.Append(key);
                    sb.Append(":");
                    for (int i = key.Length; i < keymax + 4; i++) sb.Append(" ");
                    sb.AppendLine(Tools.BeginOfString(record.GetItem(key)!.ToString() ?? "", 50));
                }
            }
            else if (obj is MOGData data)
            {
                for (int i = 0; i < data.Items.Count; i += 16)
                {
                    sb.Append(string.Format("{0:X8}  ", i));

                    for (int j = i; j < i + 16; j++)
                    {
                        var v = j < data.Items.Count ? string.Format("{0:X2}", data.Items[j]) : "  ";
                        sb.Append(string.Format("{0:X2} ", v));
                    }

                    sb.Append(" | ");

                    for (int j = i; j < i + 16; j++)
                    {
                        var c = " ";

                        if (j < data.Items.Count)
                        {
                            var v = data.Items[j];

                            if (v < 32)
                            {
                                c = ".";
                            }
                            else
                            {
                                var bytes = new byte[] { v, 0 };
                                c = BitConverter.ToChar(bytes, 0).ToString();
                            }
                        }

                        sb.Append(c);
                    }

                    sb.AppendLine("  |");
                }
            }
            else
            {
                sb.AppendLine(obj.ToString());
            }

            return sb.ToString();
        }

        #endregion

        #region FLAG FUNCTIONS

        public void FlagSet(string name) => _flags[name] = true;

        public void FlagClear(string name) => _flags.Remove(name);

        public bool FlagIsSet(string name) => _flags.ContainsKey(name);

        public bool FlagIsClear(string name) => !_flags.ContainsKey(name);

        #endregion

        #region STACK FUNCTIONS

        public void StackPush(MOGObject obj) => _currentStack.Push(obj);

        public void StackPushString(string str) => _currentStack.Push(new MOGString(this, str, 0));

        public void StackPushNumber(double number) => _currentStack.Push(new MOGNumber(this, number, 0));

        public void StackPushName(string name) => _currentStack.Push(new MOGName(this, name, 0));

        public void StackPushKey(string key) => _currentStack.Push(new MOGKey(this, key, 0));

        public void StackPushWord(string word) => _currentStack.Push(new MOGWord(this, word, 0));

        public void StackPushBoolean(bool b) => _currentStack.Push(new MOGBoolean(this, b, 0));

        public void StackPushNull() => _currentStack.Push(new MOGNull(this, 0));

        public void StackPushData(byte[] bytes) => _currentStack.Push(new MOGData(this, bytes));

        public MOGObject? StackPop()
        {
            if (_currentStack.Count == 0)
                return null;

            return _currentStack.Pop();
        }

        public List<Type> StackSign(int size)
        {
            List<Type> types = new();

            if (_currentStack.Count >= size)
            {
                MOGObject[] arr = _currentStack.ToArray();

                for (int i = 0; i < size; i++)
                    types.Add(arr[i].GetType());
            }

            return types;
        }

        public void StackClear()
        {
            _currentStack.Clear();
        }

        public MOGObject[] StackArray() => _currentStack.ToArray();

        public void AddNewStack()
        {
            _stacks.Add(new());
            _currentStack = _stacks[_stacks.Count - 1]; 
        }

        public void RemoveLastStack()
        {
            if (_stacks.Count > 1)
            {
                _stacks.RemoveAt(_stacks.Count - 1);
                _currentStack = _stacks[_stacks.Count - 1];
            }
        }

        public EvalResult StackSwap()
        {
            var stack = _currentStack;

            if (stack.Count < 2)
                return EvalResult.Failure(this, Error.TooFewArgumentsError, "swap");

            var obj1 = stack.Pop();
            var obj2 = stack.Pop();

            stack.Push(obj1);
            stack.Push((obj2));

            return EvalResult.NoError;
        }

        public EvalResult StackDup()
        {
            var stack = _currentStack;  

            if (stack.Count < 1)
                return EvalResult.Failure(this, Error.TooFewArgumentsError, "dup");

            var obj = stack.Peek();
            stack.Push(obj.Clone());

            return EvalResult.NoError;
        }

        public EvalResult StackDrop()
        {
            var stack = _currentStack;

            if (stack.Count < 1)
                return EvalResult.Failure(this, Error.TooFewArgumentsError, "drop");

            stack.Pop();

            return EvalResult.NoError;
        }

        public MOGString StackPopString() => (StackPop() as MOGString)!;

        public MOGBaseString StackPopBaseString() => (StackPop() as MOGBaseString)!;

        public MOGNumber StackPopNumber() => (StackPop() as MOGNumber)!;

        public MOGName StackPopName() => (StackPop() as MOGName)!;

        public MOGKey StackPopKey() => (StackPop() as MOGKey)!;

        public MOGList StackPopList() => (StackPop() as MOGList)!;

        public MOGRecord StackPopRecord() => (StackPop() as MOGRecord)!;

        public MOGWord StackPopWord() => (StackPop() as MOGWord)!;

        public MOGType StackPopType() => (StackPop() as MOGType)!;

        public MOGCode StackPopCode() => (StackPop() as MOGCode)!;

        public MOGFunction StackPopFunction() => (StackPop() as MOGFunction)!;

        public MOGNull StackPopNull() => (StackPop() as MOGNull)!;

        public MOGBoolean StackPopBoolean() => (StackPop() as MOGBoolean)!;

        public MOGData StackPopData() => (StackPop() as MOGData)!;

        public MOGRef StackPopRef() => (StackPop() as MOGRef)!;

        public MOGBinaryNumber StackPopBinaryNumber() => (StackPop() as MOGBinaryNumber)!;

        public string[] StackSave()
        {
            var stack = _currentStack.ToArray();
            var list = new List<string>();

            for (int i = 0; i < stack.Length; i++)
                list.Add(stack[i].ToString());

            return list.ToArray();
        }

        public EvalResult StackRestore(string[] backup)
        {
            var stack = _currentStack;

            stack.Clear();

            for (int i = backup.Length - 1; i >= 0; i--)
            {
                try
                {
                    var r = Parse(backup[i]);

                    if (r.Count > 0)
                        stack.Push(r[0]);
                }
                catch
                {
                    return EvalResult.Failure(this, Error.FatalError, "StackRestore", "unabled to restore stack backup");
                }
            }

            return EvalResult.NoError;
        }

        #endregion

        #region VARS FUNCTIONS

        public EvalResult VarWrite(string name, MOGObject value)
        {
            // This name is used by a func ?

            if (_functions.ContainsKey(name))
                return EvalResult.Failure(this, Error.NameAlreadyUsedByFunctionError);

            if (StrictMode && !VarExists(name))
                return EvalResult.Failure(this, Error.UnabledToWriteValueInUndeclaredVarError, $"variable '{name}' doesn't exist");

            bool r;

            if (name.StartsWith("$"))
            {
                // Global var

                r = _varsContext[0].Write(name, value);
            }
            else
            {
                // Local var

                r = _currentLocalVarsContext?.Write(name, value) ?? false;
            }

            if (!r)
            {
                return EvalResult.Failure(this, Error.UnabledToWriteValueError, "certainly bad type error");
            }
            else
            {
                return EvalResult.NoError;
            }
        }

        public EvalResult VarDeclare(string name, MOGObject value)
        {
            // This name is used by a func ?

            if (_functions.ContainsKey(name))
                return EvalResult.Failure(this, Error.NameAlreadyUsedByFunctionError);

            if (name.StartsWith("$"))
            {
                // Global var

                _varsContext[0].Declare(name, value);
            }
            else
            {
                // Local var

                _currentLocalVarsContext?.Declare(name, value);
            }

            return EvalResult.NoError;
        }

        public EvalResult VarDeclareForType(string name, MOGType type)
        {
            // This name is used by a func ?

            if (_functions.ContainsKey(name))
                return EvalResult.Failure(this, Error.NameAlreadyUsedByFunctionError);

            if (name.StartsWith("$"))
            {
                // Global var

                _varsContext[0].DeclareForType(name, type);
            }
            else
            {
                // Local var

                _currentLocalVarsContext?.DeclareForType(name, type);
            }

            return EvalResult.NoError;
        }

        public MOGObject? VarRead(string name, bool clone = true)
        {
            MOGObject? value = null;

            if (name.StartsWith("$"))
            {
                value = _varsContext[0].Read(name, clone);
            }
            else
            {
                value = _currentLocalVarsContext?.Read(name, clone);
            }

            return value;
        }

        public bool VarExists(string name)
        {
            if (name.StartsWith("$"))
            {
                return _varsContext[0].Exists(name);
            }
            else
            {
                return _currentLocalVarsContext?.Exists(name) ?? false;
            }
        }

        public bool VarPurge(string name)
        {
            if (name.StartsWith("$"))
            {
                return _varsContext[0].Purge(name);
            }
            else
            {
                return _currentLocalVarsContext?.Purge(name) ?? false;
            }
        }

        public void VarPushContext(string name)
        {
            _currentLocalVarsContext = new VarContext(name);
            _varsContext.Add(_currentLocalVarsContext);

        }

        public void VarPopContext()
        {
            if (_varsContext.Count > 1)
            {
                _varsContext.RemoveAt(_varsContext.Count - 1);

                if (_varsContext.Count > 1)
                {
                    _currentLocalVarsContext = _varsContext[_varsContext.Count - 1];
                }
                else
                {
                    _currentLocalVarsContext = null;
                }
            }
        }

        public string[] GetGlobalVarNames() => _varsContext[0].Keys.ToArray();

        public string[] GetLocalVarNames()
        {
            if (_varsContext.Count < 2)
                return [];

            return _currentLocalVarsContext?.Keys.ToArray() ?? [];
        }

        #endregion

        #region FUNCS FUNCTIONS

        internal EvalResult DefineFunction(string name, MOGFunction function)
        {
            if (VarExists(name))
                return EvalResult.Failure(this, Error.NameAlreadyUsedByVarError);

            if (_functions.ContainsKey(name))
                return EvalResult.Failure(this, Error.FunctionAlreadyExistsError);

            _functions[name] = function;
            return EvalResult.NoError;
        }

        internal MOGFunction? GetFunction(string name)
        {
            if (_functions.TryGetValue(name, out var function))
                return function;

            return null;
        }

        internal bool FunctionExists(string name) => _functions.ContainsKey(name);

        internal string[] GetFunctions() => _functions.Keys.ToArray();

        #endregion

        #region TIMER FUNCTIONS

        public bool TimerExists(string name) => _timers.ContainsKey(name);

        internal MOGTimer? GetTimer(string name)
        {
            if (_timers.TryGetValue(name, out var timer))
                return timer;

            return null;
        }

        internal EvalResult CreateNewTimer(string name, int interval, bool isCyclic, MOGFunction function, bool isLaterTimer = false)
        {
            if (_timers.ContainsKey(name))
                return EvalResult.Failure(this, Error.NameAlreadyExistsError, $"timer '{name}' already exists.");

            if (interval < 0)
                return EvalResult.Failure(this, Error.BadArgumentValueError, "timer interval must be a positive value.");

            var timer = new MOGTimer(this, name, interval, isCyclic, function, isLaterTimer);
            _timers[name] = timer;

            return EvalResult.NoError;
        }

        internal EvalResult PurgeTimer(string name)
        {
            if (_timers.TryGetValue(name, out var timer))
            {
                timer.Stop();
                _timers.Remove(name);
                return EvalResult.NoError;
            }

            return EvalResult.Failure(this, Error.UnknownNameError, $"unabled to purge unknown '{name}' timer.");
        }

        internal void ClearTimers()
        {
            // Stopping all timers and clear them

            foreach (var timer in _timers.Values)
                timer.Stop();

            _timers.Clear();
        }

        internal void ClearTasks()
        {
            // Stopping all tasks and clear them

            foreach (var task in _tasks.Values)
                task.Stop();

            _tasks.Clear();
        }

        internal void RegisterFireObject(MOGFireObject fireObject)
        {
            lock (_fireObjectsQueueLock)
                _fireObjectsQueue.Enqueue(fireObject);
        }

        internal void ClearWaitingFireObjects()
        {
            // Cleaning waiting fire objects function

            lock (_fireObjectsQueueLock)
                _fireObjectsQueue.Clear();
        }

        internal bool HasWaitingFireObjects => !_disableInterrupts && _fireObjectsQueue.Count > 0;

        internal async Task<EvalResult> ExecuteWaitingFireObjects()
        {
            var result = EvalResult.NoError;

            if (!_disableInterrupts && _fireObjectsQueue.Count > 0)
            {
                MOGFireObject? fireObject = null;

                lock (_fireObjectsQueueLock)
                    fireObject = _fireObjectsQueue.Dequeue();

                AddNewStack();

                result = await fireObject.Function.Execute();

                RemoveLastStack();
            }

            return result;
        }

        internal string[] TimerList() => _timers.Keys.ToArray();

        #endregion

        #region EVENT FUNCTIONS

        public bool EventExists(string name) => _events.ContainsKey(name);

        internal MOGEvent? GetEvent(string name)
        {
            if (_events.TryGetValue(name, out var @event))
                return @event;

            return null;
        }

        internal EvalResult CreateNewEvent(string name, MOGFunction function)
        {
            if (_events.ContainsKey(name))
                return EvalResult.Failure(this, Error.NameAlreadyExistsError, $"event '{name}' already exists.");

            var @event = new MOGEvent(this, name, function);
            _events[name] = @event;

            return EvalResult.NoError;
        }

        internal EvalResult EventPurge(string name)
        {
            if (_events.Remove(name))
                return EvalResult.NoError;
           
            return EvalResult.Failure(this, Error.UnknownNameError, $"unabled to purge unknown '{name}' event.");
        }

        public async Task<EvalResult> FireEvent(string name, MOGObject eventData)
        {
            try
            {
                await _fireEventSemaphore.WaitAsync();

                if (_events.TryGetValue(name, out var @event))
                {
                    var primitiveSTO = GetPrimitive(typeof(PrimitiveSTO), false);

                    if (primitiveSTO != null)
                    {
                        @event = @event.Clone();

                        @event.Function.Items.Insert(0, primitiveSTO);
                        @event.Function.Items.Insert(0, new MOGName(this, "eventData", 0));
                        @event.Function.Items.Insert(0, eventData);

                        RegisterFireObject(@event);

                        await SendEventFire(name, eventData);
                    }
                    else
                    {
                        return EvalResult.Failure(this, Error.PrimitiveSearchError, "STO");
                    }
                }

                return EvalResult.NoError;
            }
            catch
            {
                return EvalResult.Failure(this, Error.UnabledToFireEventError, $"unable to fire event '{name}'.");
            }
            finally
            {
                _fireEventSemaphore.Release();
            }
        }

        internal void ClearEvents()
        {
            _events.Clear();
        }

        internal string[] GetEvents() => _events.Keys.ToArray();

        #endregion

        #region TASK FUNCTIONS

        internal EvalResult CreateTask(string name, string code)
        {
            if (_tasks.ContainsKey(name))
                return EvalResult.Failure(this, Error.NameAlreadyExistsError);

            try
            {
                _tasks[name] = new MOGTask(this, name, code);
                return EvalResult.NoError;
            }
            catch
            {

            }

            return EvalResult.Failure(this, Error.TaskCreationError);
        }

        internal MOGTask? GetTask(string name)
        {
            if (_tasks.TryGetValue(name, out var task))
                return task;

            return null;
        }

        internal EvalResult TaskPurge(string name)
        {
            if (_tasks.TryGetValue(name, out var task))
            {
                task.Stop();
                _tasks.Remove(name);
                return EvalResult.NoError;
            }

            return EvalResult.Failure(this, Error.UnknownNameError, $"unabled to purge unknown task '{name}'");
        }

        internal async Task<EvalResult> TaskPublish(string message)
        {
            if (IsTask)
            {
                List<MOGObject>? items = null;

                try
                {
                    items = MotherEngine!.Parse(message);
                }
                catch (Exception ex)
                {
                    return EvalResult.Failure(this, Error.ParseError, ex.Message);
                }

                var messageInformations = new MOGRecord(MotherEngine!);
                messageInformations.Items["task"] = new MOGName(MotherEngine!, TaskName!, 0);
                messageInformations.Items["message"] = items[0];

                return await MotherEngine!.FireEvent(MOGTask.EVENT_TASK_DID_PUBLISH, messageInformations);
            }

            return EvalResult.NoError;
        }

        internal string[] GetTasks() => _tasks.Keys.ToArray();

        #endregion

        #region FILE FUNCTIONS

        internal void RegisterNewOpeninFile(string id, FileStream stream) => _openinFiles[id] = stream;

        internal void RegisterNewOpenoutFile(string id, FileStream stream) => _openoutFiles[id] = stream;

        internal bool OpeninFileExists(string id) => _openinFiles.ContainsKey(id);

        internal bool OpenoutFileExists(string id) => _openoutFiles.ContainsKey(id);

        internal void CloseOpeninFile(string id)
        {
            if (_openinFiles.TryGetValue(id, out var stream))
                stream.Close();
        }

        internal void CloseOpenoutFile(string id)
        {
            if (_openoutFiles.TryGetValue(id, out var stream))
                stream.Close();
        }

        internal byte[] FileRead(string id, int size)
        {
            var stream = _openinFiles[id];
            int remaining = (int)(stream.Length - stream.Position);

            if (remaining == 0)
                return [];

            if (remaining < size)
                size = remaining;

            var buffer = new byte[size];
            var len = stream.Read(buffer, 0, size);
            return buffer;
        }

        internal byte[] FileReadLine(string id)
        {
            var stream = _openinFiles[id];
            int remaining = (int)(stream.Length - stream.Position);

            if (remaining == 0)
                return [];

            var buffer = new List<byte>();

            while (true)
            {
                int b = stream.ReadByte();

                if (b == -1)
                    break;

                buffer.Add((byte)b);

                if (b == 10)
                    break;
            }

            for (int i = buffer.Count - 1; i >= 0; i--)
                if (buffer[i] == 0x0D || buffer[i] == 0x0A)
                    buffer.RemoveAt(i);

            return buffer.ToArray();
        }

        internal long OpeninFileSize(string id)
        {
            var stream = _openinFiles[id];
            return stream.Length;
        }

        internal bool FileEof(string id)
        {
            var stream = _openinFiles[id];
            return stream.Position >= stream.Length;
        }

        internal void FileWrite(string id, byte[] bytes)
        {
            _openoutFiles[id].Write(bytes);
            _openoutFiles[id].Flush();
        }

        internal void CloseOpeninFiles()
        {
            foreach (var stream in _openinFiles.Values)
                stream.Close();

            _openinFiles.Clear();
        }

        internal void CloseOpenoutFiles()
        {
            foreach (var stream in _openoutFiles.Values)
                stream.Close();

            _openoutFiles.Clear();
        }

        #endregion

        #region DEBUG FUNCTION AND PROPERTIES

        public bool DebugMode => _debugMode;

        public bool DebugPauseMode { get; set; }

        public bool DebugNextStepSignal { get; private set; }

        public bool DebugResumeSignal { get; private set; }

        public void DebugFireNextStepSignal()
        {
            if (_debugMode)
            {
                lock (_debugNextStepSignalLock)
                {
                    DebugNextStepSignal = true;
                }
            }
        }

        public void DebugExtinguishSignals()
        {
            lock (_debugNextStepSignalLock)
            {
                DebugNextStepSignal = false;
            }

            lock (_debugResumeSignalLock)
            {
                DebugResumeSignal = false;
            }
        }

        public void DebugFireResumeSignal()
        {
            if (_debugMode)
            {
                lock (_debugResumeSignalLock)
                {
                    DebugResumeSignal = true;
                }
            }
        }

        #endregion

        #region NETWORK FUNCTIONS

        public async Task StartNetworkCommunication(string address = "0.0.0.0", int port = 1968)
        {
            // We activate the UDP presence server on the network

            StartDatagramServer(port);

            // We activate the TCP/IP server for the studio

            await StartSocketServerAsync(address);
        }

        #endregion

        #region DATAGRAM SERVER

        public void StartDatagramServer(int port)
        {
            if (!_datagramManager.IsRunning)
            {
                _datagramManager.Start(_name, port);

                _datagramManager.ManagerDidStart += DatagramManager_ManagerDidStart;
                _datagramManager.ManagerDidStop += DatagramManager_ManagerDidStop;
                _datagramManager.DatagramDidReceive += DatagramManager_DatagramDidReceive;
            }
        }

        public void StopDatagramServer()
        {
            _datagramManager.Stop();

            _datagramManager.ManagerDidStart -= DatagramManager_ManagerDidStart;
            _datagramManager.ManagerDidStop -= DatagramManager_ManagerDidStop;
            _datagramManager.DatagramDidReceive += DatagramManager_DatagramDidReceive;
        }

        private void DatagramManager_DatagramDidReceive(System.Net.IPEndPoint from, byte[] data)
        {
            try
            {
                var s = Encoding.UTF8.GetString(data);
                // var message = JsonSerializer.Deserialize<ServerMessage>(s);
                var message = JsonSerializer.Deserialize(s, MogwaiJsonContext.Default.ServerMessage);

                if (message != null && message.Source == "MOGWAI STUDIO")
                {
                    // Message en provenance du studio de développement
                    // On le traite

                    switch (message.Function)
                    {
                        case "WHO IS HERE":
                            _ = StudioRequestWhoIsHere(message, from);
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        private void DatagramManager_ManagerDidStop()
        {
            _datagramManager.ManagerDidStart -= DatagramManager_ManagerDidStart;
            _datagramManager.ManagerDidStop -= DatagramManager_ManagerDidStop;
            _datagramManager.DatagramDidReceive -= DatagramManager_DatagramDidReceive;
        }

        private void DatagramManager_ManagerDidStart()
        {

        }

        private async Task StudioRequestWhoIsHere(ServerMessage message, System.Net.IPEndPoint from)
        {
            // Un studio demande qui est présent sur le réseau
            // On lui répond QUE si on n'est pas déjà connecté en TCP/IP avec un studio
            // Dans les paramètres on donnera des infos sur le runtime en présence
            // mogwai.infos

            if (_socketServerService.IsRunning) return;

            try
            {
                var infos = GetRuntimeInformationsForConnection();
                var msg = new ServerMessage("MOGWAI RUNTIME", "I AM HERE", infos);
                //var mser = JsonSerializer.Serialize(msg);
                var mser = JsonSerializer.Serialize(msg, MogwaiJsonContext.Default.ServerMessage);
                var bytes = Encoding.UTF8.GetBytes(mser);

                _datagramManager.SendDatagram(from.Address.ToString(), from.Port, bytes);
            }
            catch (Exception ex)
            {
                if (Delegate != null)
                {
                    await Delegate.ConsolePrintLn(this, $"\nWHO IS HERE STUDIO REQUEST ERROR !\n{ex.Message}\n");
                }
            }
        }

        private string[] GetRuntimeInformationsForConnection()
        {
            // I AM HERE
            // P0 = name;
            // P1 = debug port;
            // P2 = mogwai:
            // P3 = platform:
            // P4 = architecture: 
            // P5 = OSdescription:
            // P6 = framework:
            // P7 = skills: (\t separator)
            // P8 = public primitives: name categorie\t
            // P9 = externals primitives: (\t separator)

            var infos = GetVersionInformations();

            var allKeywords = string.Empty;
            var externalsKeywords = string.Empty;

            var sb = new StringBuilder();

            foreach (var key in Primitives)
            {
                var p = _primitivesByName[key];

                sb.Append(p.Name);
                sb.Append(" ");
                sb.Append(p.Category);
                sb.Append("\t");
            }

            allKeywords = sb.ToString();

            if (Delegate != null)
            {
                var kws = new StringBuilder();

                foreach (var k in Delegate.HostFunctions(this))
                {
                    if (kws.Length > 0) kws.Append("\t");
                    kws.Append(k);
                }

                externalsKeywords = kws.ToString();
            }

            var allSkills = string.Empty;

            string[] response =
            [
                _datagramManager.Name,
                _socketServerService.Port.ToString(),
                infos["mogwai"],
                infos["platform"],
                infos["architecture"],
                infos["OSdescription"],
                infos["framework"],
                allSkills,
                allKeywords,
                externalsKeywords
            ];

            return response;
        }

        #endregion

        #region SOCKET SERVER

        public async Task StartSocketServerAsync(string address)
        {
            if (!_socketServerService.IsRunning)
            {
                try
                {
                    var r = await _socketServerService.StartServerAsync(this, address, _debugPort);

                    if (r)
                    {
                        _socketServerService.MessageDidReceive += _SocketServerService_MessageReceived;
                        _socketServerService.ServerDidDisconnect += _SocketServerService_ServerDidDisconnect;

                        if (Delegate != null)
                        {
                            await Delegate.SocketServerDidStart(this, _socketServerService.IpAddress!, _socketServerService.Port);
                        }
                    }
                }
                catch
                {
                    if (Delegate != null)
                    {
                        await Delegate.ConsolePrintLn(this, "\nUnable to wait for incoming connection !\n");
                    }
                }
            }
        }

        public async Task StopSocketServer()
        {
            await StopRuntime();
        }

        private void _SocketServerService_ServerDidDisconnect(object sender)
        {
            _socketServerService.MessageDidReceive -= _SocketServerService_MessageReceived;
            _socketServerService.ServerDidDisconnect -= _SocketServerService_ServerDidDisconnect;

            Delegate?.SocketServerDidStop(this);
        }

        private async void _SocketServerService_MessageReceived(object sender, ServerMessage message)
        {
            // On vient de recevoir un message du studio
            // On ne répond qu'à la source MOGWAI STUDIO 

            if (message.Source == "MOGWAI STUDIO")
            {
                // On traite le message

                switch (message.Function)
                {
                    case "RUN":
                        StudioRequestRun(message.Parameters);
                        break;

                    case "DEBUG":
                        StudioRequestRunDebug(message.Parameters);
                        break;

                    case "DIRECT COMMAND":
                        StudioRequestDirectCommand(message.Parameters);
                        break;

                    case "DEBUG ON":
                        _debugMode = true;
                        break;

                    case "DEBUG OFF":
                        _debugMode = false;
                        break;

                    case "TRON":
                        StudioRequestTron(message.Parameters);
                        break;

                    case "TROFF":
                        // Troff();
                        break;

                    case "HALT":
                        Halt();
                        break;

                    case "PAUSE":
                        DebugPauseMode = true;
                        break;

                    case "STEP":
                        DebugFireNextStepSignal();
                        break;

                    case "RESUME":
                        DebugFireResumeSignal();
                        break;

                    case "EXIT":
                        await StopRuntime();
                        break;

                    case "SHELL":
                        StudioRequestShell(message.Parameters);
                        break;

                    case "?RUNTIME":
                        await StudioRequestRuntimeInformations();
                        break;

                    case "?KEYWORDS":
                        await StudioRequestKeywords();
                        break;

                    case "?STACK":
                        await StudioRequestStackInformations();
                        break;

                    case "?CALL TRACE":
                        StudioRequestCallTrace();
                        break;

                    case "CLEAR STACK":
                        await StudioRequestClearStack();
                        break;

                    case "?VARS":
                        await StudioRequestGlobalVarsInformations();
                        break;

                    case "?LVARS":
                        await StudioRequestLocalVarsInformations();
                        break;

                    case "?TASKS":
                        await StudioRequestTasksInformations();
                        break;

                    case "?FUNCS":
                        await StudioRequestFuncs(message.Parameters);
                        break;

                    case "?INSTANCES":
                        StudioRequestInstancesInformations();
                        break;

                    case "ADD BREAK POINT":
                        StudioRequestAddBreakPoint(message.Parameters);
                        break;

                    case "MAKE MOGX":
                        await StudioRequestMakeMOX(message.Parameters);
                        break;

                    case "?HELP":
                        StudioRequestHelp(message.Parameters);
                        break;

                    case "CONSOLE":
                        StudioRequestConsole(message.Parameters);
                        break;
                }
            }
        }

        private async Task StopRuntime()
        {
            if (_socketServerService != null)
            {
                await _socketServerService.SendToClientAsync("STOP RUNTIME");
                _socketServerService.StopServer();
            }
        }

        private void StudioRequestRun(List<string> parameters)
        {
            if (parameters.Count > 0 && !IsRunning)
            {
                var code = parameters[0] ?? "";
                _ = RunAsync(code, false);
            }
        }

        private void StudioRequestRunDebug(List<string> parameters)
        {
            if (parameters.Count > 0 && !IsRunning)
            {
                var code = parameters[0] ?? "";
                _ = RunAsync(code, true);
            }
        }

        private void StudioRequestDirectCommand(List<string> parameters)
        {
            if (parameters.Count > 0 && !IsRunning)
            {
                var code = parameters[0] ?? "";
                _ = RunAsync(code, false);
            }
        }

        private void StudioRequestTron(List<string> parameters)
        {
            if (parameters.Count > 0)
            {
                if (int.TryParse(parameters[0], out int v))
                {
                    // Tron(v);
                }
            }
        }

        private void StudioRequestShell(List<string> parameters)
        {
            /*
            if (parameters.Count > 0)
            {
                var command = parameters[0];

                if (command != null)
                {
                    Delegate?.ConsoleWriteLine(this, ">" + command);
                    var r = RunPauseCommand(command);
                    if (r.Error != NoError) Delegate?.ConsoleWriteLine(this, r.ToString());
                }
            }
            */
        }

        private void StudioRequestAddBreakPoint(List<string> parameters)
        {
            // P0 = WORD
            // P1 = START
            // P2 = END

            /*
            if (parameters.Count > 2 && int.TryParse(parameters[1], out int start) && int.TryParse(parameters[2], out int end))
            {
                AddBreakPoint(start, end);
            }
            */
        }

        private async Task StudioRequestMakeMOX(List<string> parameters)
        {
            // P0 = GUID
            // P1 = CODE

            if (parameters.Count > 1)
            {
                await SendDebugMessage("Generate MOX...");

                var name = parameters[0];
                var content = parameters[1];

                try
                {
                    var code = new MOGCode(this, content, 0, null);
                    var mox = CreateMOX(code);
                    var b64 = Convert.ToBase64String(mox!);

                    if (_socketServerService != null)
                        await _socketServerService.SendToClientAsync("MOGX", name, b64);
                }
                catch
                {
                    if (_socketServerService != null)
                        await _socketServerService.SendToClientAsync("MOGX", name, "ERROR!");

                    await SendDebugMessage("MOGX error !");
                }
            }
        }

        private void StudioRequestHelp(List<string> parameters)
        {
            // P0 = keyword

            /*
            if (parameters.Count > 0)
            {
                var content = await GetHelpForWordAsync(parameters[0]);
                _SocketServerService?.SendToClientAsync("HELP", parameters[0], content);
            }
            */
        }

        private void StudioRequestConsole(List<string> parameters)
        {
            // P0 = ACTIVATE or DEACTIVATE

            /*
            if (parameters.Count > 0)
            {
                switch (parameters[0])
                {
                    case "ACTIVATE":
                        ReportConsoleActionsToStudio = true;
                        break;

                    case "DEACTIVATE":
                        ReportConsoleActionsToStudio = false;
                        break;
                }
            }
            */
        }

        private async Task StudioRequestRuntimeInformations()
        {
            // On envoie au client des infos sur le runtime
            // Dans le même format que la découverte I AM HERE

            if (_socketServerService != null)
            {
                var parameters = GetRuntimeInformationsForConnection();
                await _socketServerService.SendToClientAsync("RUNTIME", parameters);
            }
        }

        private async Task StudioRequestKeywords()
        {
            // On evoie les keywords au studio (uniquement les publics)

            // KEYWORDS
            // P0 = natives keywords séparés par un espace
            // P1 = extensions keywords
            // P2 = externals keywords

            if (_socketServerService != null)
            {
                var keys = GetPublicPrimitiveNames();
                var ks1 = string.Join(" ", keys);

                var kwex = new List<string>();
                var ks2 = string.Empty;

                /*
                foreach (var ext in _Extensions.Values)
                    kwex.AddRange(ext.Keywords);

                var ks2 = string.Join(" ", kwex);
                */

                var ks3 = string.Empty;

                if (Delegate != null)
                    ks3 = string.Join(" ", Delegate.HostFunctions(this));

                await _socketServerService.SendToClientAsync("KEYWORDS", ks1, ks2, ks3);
            }
        }

        private void SendTronModeValidated()
        {
            _socketServerService?.SendToClientAsync("TRON OK").Wait();
        }

        private void sendTroffModeValidated()
        {
            _socketServerService?.SendToClientAsync("TROFF OK").Wait();
        }

        private void SendParseStart()
        {
            _socketServerService?.SendToClientAsync("PARSE START").Wait();
        }

        private void SendParseEnd()
        {
            _socketServerService?.SendToClientAsync("PARSE END").Wait();
        }

        private void sendParseError(EvalResult r)
        {
            /*
            var startPos = -1;
            var endPos = -1;

            if (r.Reason != null)
            {
                startPos = r.Reason.StartPos;
                endPos = r.Reason.EndPos;
            }

            _SocketServerService?.SendToClientAsync(
                "PARSE ERROR",
                r.Error.Number.ToString(),
                r.ToString(),
                startPos.ToString(),
                endPos.ToString(),
                r.Reason?.Source ?? ""
                ).Wait();
            */
        }

        private async Task SendProgramStart(bool debugMode)
        {
            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("PRG START", debugMode ? "1" : "0");
        }

        private async Task SendProgramEndWithoutError(EvalResult r)
        {
            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("PRG STOP", r.Duration.TotalMilliseconds.ToString());
        }

        private async Task SendProgramEndWithError(EvalResult r)
        {
            if (_socketServerService != null)
            {
                await _socketServerService.SendToClientAsync(
                    "PRG ERROR",
                    r.Error.Code,
                    r.ToString(),
                    r.Duration.TotalMilliseconds.ToString(),
                    r.StartErrorPosition.ToString(),
                    r.EndErrorPosition.ToString(),
                    r.ExecutionContext?.CodeFilename ?? ""
                    );
            }
        }

        public async Task SendProgramInformations(MOGObject currentObject, string? sourcePath)
        {
            Debug.WriteLine($"{currentObject} xpos={currentObject.StartPos} ypos={currentObject.EndPos}");

            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync(
                    "PRG INFO",
                    sourcePath ?? "",
                    currentObject.ToString()!,
                    currentObject.Type?.ToString() ?? "?",
                    currentObject.StartPos.ToString(),
                    currentObject.EndPos.ToString());
        }

        private void StudioRequestCallTrace()
        {
            /*
            // On prépare le tableau des lignes de la call trace

            string[] items = new string[_CallTrace.Count];

            for (int i = 0; i < _CallTrace.Count; i++)
            {
                var item = _CallTrace[i];
                var sb = new StringBuilder(100);

                sb.Append(item.StartPos).Append((char)1);
                sb.Append(item.EndPos).Append((char)1);
                sb.Append(item.Type).Append((char)1);
                sb.Append(item.ToText());

                items[i] = sb.ToString();
            }

            _SocketServerService?.SendToClientAsync("CALL TRACE", items);
            */
        }

        internal async Task SendDebugMessage(string message)
        {
            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("DEBUG MSG", message);
        }

        internal async Task SendDebugClear()
        {
            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("DEBUG CLR");
        }
        
        private async Task SendUsingExtension(IPlugin plugin)
        {
            if (_socketServerService.IsRunning)
            {
                // USING EXTENSION
                // P0 = name
                // P1 = keywords (sépararés par un espace)

                var kws = plugin.Keywords.Keys.ToArray();
                await _socketServerService.SendToClientAsync("USING EXTENSION", plugin.Name, string.Join(' ', kws));
            }
        }

        public async Task SendTrace()
        {
            // On envoie le signal de trace

            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("TRACE");
        }

        private async Task StudioRequestStackInformations()
        {
            // STACK
            // P0 = item0
            // P1 = item1
            // PN = itemN

            if (_socketServerService != null)
            {
                var lst = new List<string>();
                var items = _currentStack.ToArray();

                foreach (var item in items)
                    lst.Add(item.ToString());

                await _socketServerService.SendToClientAsync("STACK", lst.ToArray());
            }
        }

        private async Task StudioRequestGlobalVarsInformations()
        {
            // VARS
            // P0 = item0
            // P1 = item1
            // PN = itemN

            if (_socketServerService != null)
            {
                var lst = new List<string>();

                foreach (var name in _varsContext[0].Keys)
                {
                    var v = _varsContext[0].Read(name);

                    if (v != null)
                    {
                        var type = v.Type;
                        var value = v.ToString();

                        lst.Add($"{name}\t{type}\t{value}");
                    }
                }

                await _socketServerService.SendToClientAsync("VARS", lst.ToArray());
            }
        }

        private async Task StudioRequestLocalVarsInformations()
        {
            // LVARS
            // P0 = item0
            // P1 = item1
            // PN = itemN

            if (_currentLocalVarsContext != null)
            {
                var lst = new List<string>();

                foreach (var name in _currentLocalVarsContext.Keys)
                {
                    var v = _currentLocalVarsContext.Read(name);

                    if (v != null)
                    {
                        var type = v.Type;
                        var value = v.ToString();

                        lst.Add($"{name}\t{type}\t{value}");
                    }
                }

                await _socketServerService.SendToClientAsync("LVARS", lst.ToArray());
            }
        }

        private async Task StudioRequestTasksInformations()
        {
            // TASKS
            // P0 = taskname \t taskstatus
            // P1 =
            // PN =

            if (_socketServerService != null)
            {
                var tasks = _tasks.Values.ToArray();
                var lst = new List<string>();

                foreach (var t in tasks)
                    lst.Add($"{t.Name}\t{t.Status}");

                await _socketServerService.SendToClientAsync("TASKS", lst.ToArray());
            }
        }

        private async Task StudioRequestFuncs(List<string> parameters)
        {
            // FUNCS
            // P0 = funcname \t start \t end
            // P1 =
            // PN =

            if (_socketServerService != null && parameters.Count > 0)
            {    
                var r = GetFuncNames(parameters[0]);

                if (r.result.IsSuccess)
                {
                    var funcs = new List<string>();

                    foreach (var name in r.funcNames)
                        funcs.Add($"{name.Value}\t{name.StartPos}\t{name.EndPos}");

                    await _socketServerService.SendToClientAsync("FUNCS", funcs.ToArray());
                }
            }
        }

        private void StudioRequestInstancesInformations()
        {
            // INSTANCES
            // P0 = reference \t class \t usage counter
            // P1 =
            // PN =

            /*
            lock (_Instances)
            {
                _SocketServerService?.SendToClientAsync("INSTANCES", GetInstances().ToArray()).Wait();
            }
            */
        }

        private async Task StudioRequestClearStack()
        {
            StackClear();
            await StudioRequestStackInformations();
        }

        internal async Task SendProgramPause()
        {
            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("PRG PAUSE");
        }

        internal async Task SendProgramResume()
        {
            if (_socketServerService != null)
                await _socketServerService.SendToClientAsync("PRG RESUME");
        }

        internal async Task SendEventFire(string eventName, MOGObject? eventData)
        {
            if (_socketServerService != null)
            {
                var now = DateTime.Now;
                await _socketServerService.SendToClientAsync("EVENT", eventName, $"{now.ToShortDateString()} {now.ToLongTimeString()}", eventData?.ToString() ?? "");
            }
        }

        #endregion

        #region INTERNALS FUNCTIONS

        internal double GetNexRandomValue() => _random.NextDouble();

        internal bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var c1 = name[0];
            var c2 = name.Length > 1 ? name[1] : '\0';

            if (!char.IsLetter(c1) && c1 != '_' && c1 != '$' && (c1 != '-' || c2 != '>'))
                return false;

            if (_primitivesByName.ContainsKey(name))
                return false;

            if (_hostFunctions.Contains(name))
                return false;

            var invalid = AllowExtendedNames ? _invalidCharsExtended : _invalidChars;
            return name.IndexOfAny(invalid) == -1;
        }

        internal MOGType GetType(Type type)
        {
            if (_types.TryGetValue(type, out var mogType))
                return mogType;

            return _typeAny;
        }

        internal MOGType? GetType(string name)
        {
            if (_typesByName.TryGetValue(name, out var mogType))
                return mogType;

            return null;
        }

        internal async Task<EvalResult> Include(string codeFile)
        {
            var allow = AllowPrivatePrimitives;

            try
            {
                MOGFunction? function = null;

                var filename = Path.GetFileName(codeFile);

                if (codeFile == filename)
                {
                    // On a donné un fichier sans chemin
                    // On utilise dans ce cas le répertoire des programmes

                    codeFile = Path.Combine(ProgramsDirectory, codeFile);
                }
                else
                {
                    codeFile = Path.GetFullPath(codeFile);
                }

                var hash = codeFile.GetHashCode();

                if (_includes.ContainsKey(hash))
                {
                    function = _includes[hash].Function;
                }
                else
                {
                    var bytes = File.ReadAllBytes(codeFile);
                    var result = GetCodeFormBytes(bytes);

                    if (result.code != null)
                    {
                        var context = new MogwaiExecutionContext(codeFile, result.code, hash, result.origin == CodeOrigin.Mog);
                        AllowPrivatePrimitives = true;
                        function = new MOGFunction(this, result.code, 0, context);
                        context.Function = function;
                        AllowPrivatePrimitives = allow;
                        _includes[hash] = context;
                    }
                    else
                    {
                        return EvalResult.Failure(this, Error.ParseError, "include");
                    }
                }

                return await function!.Execute();
            }
            catch
            {
                AllowPrivatePrimitives = allow;
                return EvalResult.Failure(this, Error.ParseError, "include");
            }
        }

        internal void ClearIncludes() => _includes.Clear();

        internal byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();

            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
                dstream.Write(data, 0, data.Length);

            return output.ToArray();
        }

        internal byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();

            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                dstream.CopyTo(output);

            return output.ToArray();
        }

        internal MOGObject? ObjectFromJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    var s = element.GetString();

                    if (s != null)
                    {
                        if (s.StartsWith("'") && s.EndsWith("'"))
                        {
                            return new MOGName(this, s.Replace("'", ""));
                        }
                        else if (s.EndsWith(":"))
                        {
                            return new MOGKey(this, s.Substring(0, s.Length - 1));
                        }
                        else
                        {
                            return new MOGString(this, s);
                        }
                    }
                    else
                    {
                        return new MOGString(this, "");
                    }

                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                    {
                        return new MOGNumber(this, intValue);
                    }
                    else if (element.TryGetDouble(out double doubleValue))
                    {
                        return new MOGNumber(this, doubleValue);
                    }
                    else
                    {
                        return new MOGString(this, element.GetRawText());
                    }

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return new MOGBoolean(this, element.GetBoolean());

                case JsonValueKind.Array:
                    var array = new MOGList(this);
                    foreach (var item in element.EnumerateArray())
                    {
                        var obj = ObjectFromJsonElement(item);

                        if (obj != null)
                            array.Items.Add(obj);
                    }
                    return array;

                case JsonValueKind.Object:
                    var record = new MOGRecord(this);
                    foreach (var item in element.EnumerateObject())
                    {
                        var value = ObjectFromJsonElement(item.Value);

                        if (value != null)
                            record.Items[item.Name] = value;
                    }
                    return record;

                case JsonValueKind.Null:
                    return new MOGNull(this);

                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }

        internal byte[]? CreateMOX(MOGCode code)
        {
            try
            {
                string s = code.ToStringCode();
                byte[] t = Encoding.UTF8.GetBytes(s);
                byte[] b = Tools.Compress(t);

                byte[] bytes = new byte[b.Length + 12];

                Array.Copy(_MOXSign, bytes, 12);
                Array.Copy(b, 0, bytes, 12, b.Length);

                return bytes;
            }
            catch
            {
                return null;
            }
        }

        internal string? GetCodeFromMOX(byte[] mox)
        {
            try
            {
                if (mox.Length > 12)
                {
                    byte[] e0 = new byte[12];
                    Array.Copy(mox, e0, 12);

                    for (int i = 0; i < 12; i++)
                        if (e0[i] != _MOXSign[i])
                            return null;

                    byte[] e1 = new byte[mox.Length - 12];
                    Array.Copy(mox, 12, e1, 0, e1.Length);

                    byte[] t = Tools.Decompress(e1);
                    return Encoding.UTF8.GetString(t);
                }
            }
            catch
            {

            }

            return null;
        }

        [UnconditionalSuppressMessage("AOT", "IL2026", Justification = "Plugin system requires dynamic assembly loading by design.")]
        [UnconditionalSuppressMessage("AOT", "IL2072", Justification = "Plugin system requires dynamic type instantiation by design.")]

        internal async Task<EvalResult> Using(string path)
        {
            try
            {
                PluginLoadContext loadContext = new PluginLoadContext(path);

                var assembly = loadContext.LoadFromAssemblyPath(path);

                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            if (string.IsNullOrEmpty(plugin.ID))
                            {
                                return EvalResult.Failure(this, Error.UsingError, "using ID is null or empty");
                            }
                            else if (string.IsNullOrEmpty(plugin.Name))
                            {
                                return EvalResult.Failure(this, Error.UsingError, plugin.ID, "using Name is null or empty");
                            }
                            else if (plugin.Version == null)
                            {
                                return EvalResult.Failure(this, Error.UsingError, plugin.Name, "using Version is null or empty");
                            }
                            else if (string.IsNullOrEmpty(plugin.Namespace))
                            {
                                return EvalResult.Failure(this, Error.UsingError, plugin.Name, "using Namespace is null or empty");
                            }
                            else if (_plugins.ContainsKey(plugin.ID))
                            {
                                // Ce plugin est déjà chargé
                                // On ignore la commande tout simplement

                                return EvalResult.NoError;
                            }

                            if (await plugin.Initialize(this))
                            {
                                _plugins[plugin.ID] = new PluginInformations(plugin, loadContext);

                                await SendUsingExtension(plugin);
                            }
                            else
                            {
                                return EvalResult.Failure(this, Error.UsingError, plugin.Name, "using initialization error !");
                            }
                        }
                    }
                }

                return EvalResult.NoError;
            }
            catch (Exception ex)
            {
                return EvalResult.Failure(this, Error.UsingError, ex.Message);
            }
        }

        internal async Task<EvalResult> ExecutePluginKeyword(string keyword)
        {
            foreach (var pluginInformations in _plugins.Values)
            {
                var f = pluginInformations.GetKeyword(keyword);

                if (f != null)
                    return await f(this, keyword);
            }

            return EvalResult.NoPluginFunction;
        }

        internal async Task ClearUsings()
        {
            foreach (var infos in PluginInformations)
            {
                if (infos.IsUnloadable)
                {
                    await infos.Plugin.Dispose(this);

                    infos.LoadContext.Unload();

                    _plugins.Remove(infos.ID);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        internal void ResetUsings()
        {
            foreach (var p in PluginInformations)
                p.Plugin.Reset(this);
        }

        internal bool CheckCodeFootprint(List<MOGObject> objects, int startIndex, params string?[] footPrint)
        {
            for (int i = 0; i < footPrint.Length; i++)
            {
                if (i >= footPrint.Length)
                    return false;

                var f = footPrint[i];

                if (f == null)
                    continue;

                if (objects[startIndex + i].ToString() == f)
                    continue;

                return false;
            }

            return true;
        }

        internal bool RegisterVarAutoEval(string name)
        {
            if (_varsInAutoEval.Contains(name))
                return false;

            _varsInAutoEval.Add(name);

            return true;
        }

        internal bool UnregisterVarAutoEval(string name) => _varsInAutoEval.Remove(name);

        internal bool IsJustFilename(string path) => Path.GetDirectoryName(path) == string.Empty;

        #endregion

        #region PUBLIC FUNCTIONS

        public List<MOGObject> Parse(string code)
        {
            _parser.Parse(this, code, 0, null);
            return _parser.ParsedObjects;
        }

        public async Task<EvalResult> RunAsync(string code, bool debugMode)
        {
            var result = await Task<EvalResult>.Run(async () =>
            {
                return await ExecuteAsync(code, debugMode);
            });

            return result;
        }

        public (CodeOrigin origin, string? code) GetCodeFormBytes(byte[] bytes)
        {
            // Transforme en MOGCode un tableau d'octets qui est soit un MOX soit du code source
            // Retourne un eval result et le MOGCode

            var code = GetCodeFromMOX(bytes);

            if (code != null)
                return (CodeOrigin.Mox, code);

            try
            {
                return (CodeOrigin.Mog, Encoding.UTF8.GetString(bytes));
            }
            catch
            {

            }

            return (CodeOrigin.Unknown, null);
        }

        public void CreateBreakRequest()
        {
            _breakRequested.Insert(0, false);
        }

        public void SetBreakRequest()
        {
            if (_breakRequested.Count > 0)
                _breakRequested[0] = true;
        }

        public void RemoveBreakRequest()
        {
            if (_breakRequested.Count > 0)
                _breakRequested.RemoveAt(0);
        }

        public void Halt()
        {
            HaltRequested = true;
        }

        public async Task Reset(bool keepAlive)
        {
            // Clear stack

            _stacks.Clear();
            _stacks.Add(new());

            // Stop and Clear all timers

            ClearTimers();

            // Clear all events

            ClearEvents();

            // Stop and Clear all tasks

            ClearTasks();

            // Clear all waiting fire functions 

            ClearWaitingFireObjects();

            // Reset all usings

            ResetUsings();

            // Enable interrups

            EnableInterrupts();

            // Clear circular references list

            _varsInAutoEval.Clear();

            // Troff

            TronValue = 0;

            if (!keepAlive)
            {
                // Strict mode OFF

                StrictMode = false;

                // Close all openin files

                CloseOpeninFiles();

                // Close all openout files

                CloseOpenoutFiles();

                // Clear using extensions

                await ClearUsings();

                // Clear using errors

                Error.ClearUsingsErrors();

                // Clear last error

                LastError = Error.None;

                // Clear vars

                _varsContext[0].Clear();

                // Clear flags

                _flags.Clear();

                // Clear functions

                _functions.Clear();
            }
        }

        public void DisableInterrupts() => _disableInterrupts = true;

        public void EnableInterrupts() => _disableInterrupts = false;

        public Error RegisterError(IPlugin plugin, string code, string message)
        {
            var c = $"{plugin.Namespace}.{code}";
            var error = Error.RegisterError(c, message, Error.ErrorType.Using);
            return error;
        }

        public Error RegisterError(IDelegate @delegate, string code, string message)
        {
            var c = $"HOST.{code}";
            var error = Error.RegisterError(c, message, Error.ErrorType.Using);
            return error;
        }

        public (EvalResult result, List<MOGName> funcNames) GetFuncNames(string code)
        {
            var defuncs = new List<MOGName>();

            void ExtractDefuncs(List<MOGObject> objects)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is PrimitiveDEFUNC && i > 0 && objects[i - 1] is MOGName name)
                    {
                        defuncs.Add(name);
                    }
                    else if (objects[i] is MOGBaseItems mogItems)
                    {
                        ExtractDefuncs(mogItems.Items);
                    }
                }
            }

            try
            {
                var parser = new Parser();
                parser.Parse(this, code, 0, null);

                ExtractDefuncs(parser.ParsedObjects); 
                
                return (EvalResult.NoError, defuncs);
            }
            catch (Exception ex)    
            {
                return (EvalResult.ParseFailure(this, ex.Message), defuncs);
            }          
        }

        #endregion
    }
}

from __future__ import annotations
import ctypes
import typing

T = typing.TypeVar("T")
c_lib = None

def init_lib(path):
    """Initializes the native library. Must be called at least once before anything else."""
    global c_lib
    c_lib = ctypes.cdll.LoadLibrary(path)

    c_lib.calculator_destroy.argtypes = [ctypes.POINTER(ctypes.c_void_p)]
    c_lib.calculator_new.argtypes = [ctypes.POINTER(ctypes.c_void_p), ctypes.POINTER(ctypes.c_char)]
    c_lib.calculator_calculate.argtypes = [ctypes.c_void_p, ERROR]
    c_lib.score_params_destroy.argtypes = [ctypes.POINTER(ctypes.c_void_p)]
    c_lib.score_params_new.argtypes = [ctypes.POINTER(ctypes.c_void_p)]
    c_lib.score_params_mode.argtypes = [ctypes.c_void_p, ctypes.c_int]
    c_lib.score_params_mods.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_acc.argtypes = [ctypes.c_void_p, ctypes.c_double]
    c_lib.score_params_n300.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_n100.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_n50.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_combo.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_score.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_n_misses.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_n_katu.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_passed_objects.argtypes = [ctypes.c_void_p, ctypes.c_uint32]
    c_lib.score_params_clock_rate.argtypes = [ctypes.c_void_p, ctypes.c_double]

    c_lib.calculator_destroy.restype = ctypes.c_int
    c_lib.calculator_new.restype = ctypes.c_int
    c_lib.calculator_calculate.restype = CalculateResult
    c_lib.score_params_destroy.restype = ctypes.c_int
    c_lib.score_params_new.restype = ctypes.c_int

    c_lib.calculator_destroy.errcheck = lambda rval, _fptr, _args: _errcheck(rval, 0)
    c_lib.calculator_new.errcheck = lambda rval, _fptr, _args: _errcheck(rval, 0)
    c_lib.score_params_destroy.errcheck = lambda rval, _fptr, _args: _errcheck(rval, 0)
    c_lib.score_params_new.errcheck = lambda rval, _fptr, _args: _errcheck(rval, 0)






TRUE = ctypes.c_uint8(1)
FALSE = ctypes.c_uint8(0)


def _errcheck(returned, success):
    """Checks for FFIErrors and converts them to an exception."""
    if returned == success: return
    else: raise Exception(f"Function returned error: {returned}")


class CallbackVars(object):
    """Helper to be used `lambda x: setattr(cv, "x", x)` when getting values from callbacks."""
    def __str__(self):
        rval = ""
        for var in  filter(lambda x: "__" not in x, dir(self)):
            rval += f"{var}: {getattr(self, var)}"
        return rval


class _Iter(object):
    """Helper for slice iterators."""
    def __init__(self, target):
        self.i = 0
        self.target = target

    def __iter__(self):
        self.i = 0
        return self

    def __next__(self):
        if self.i >= self.target.len:
            raise StopIteration()
        rval = self.target[self.i]
        self.i += 1
        return rval


class Mode:
    #  osu!standard
    Osu = 0
    #  osu!taiko
    Taiko = 1
    #  osu!catch
    Catch = 2
    #  osu!mania
    Mania = 3


class FFIError:
    Ok = 0
    Null = 100
    Panic = 200
    Fail = 300


class Optionf64(ctypes.Structure):
    """May optionally hold a value."""

    _fields_ = [
        ("_t", ctypes.c_double),
        ("_is_some", ctypes.c_uint8),
    ]

    @property
    def value(self) -> ctypes.c_double:
        """Returns the value if it exists, or None."""
        if self._is_some == 1:
            return self._t
        else:
            return None

    def is_some(self) -> bool:
        """Returns true if the value exists."""
        return self._is_some == 1

    def is_none(self) -> bool:
        """Returns true if the value does not exist."""
        return self._is_some != 0


class Optionu32(ctypes.Structure):
    """May optionally hold a value."""

    _fields_ = [
        ("_t", ctypes.c_uint32),
        ("_is_some", ctypes.c_uint8),
    ]

    @property
    def value(self) -> ctypes.c_uint32:
        """Returns the value if it exists, or None."""
        if self._is_some == 1:
            return self._t
        else:
            return None

    def is_some(self) -> bool:
        """Returns true if the value exists."""
        return self._is_some == 1

    def is_none(self) -> bool:
        """Returns true if the value does not exist."""
        return self._is_some != 0


class CalculateResult(ctypes.Structure):

    # These fields represent the underlying C data layout
    _fields_ = [
        ("mode", ctypes.c_uint8),
        ("stars", ctypes.c_double),
        ("pp", ctypes.c_double),
        ("ppAcc", Optionf64),
        ("ppAim", Optionf64),
        ("ppFlashlight", Optionf64),
        ("ppSpeed", Optionf64),
        ("ppStrain", Optionf64),
        ("nFruits", Optionu32),
        ("nDroplets", Optionu32),
        ("nTinyDroplets", Optionu32),
        ("aimStrain", Optionf64),
        ("speedStrain", Optionf64),
        ("flashlightRating", Optionf64),
        ("sliderFactor", Optionf64),
        ("ar", ctypes.c_double),
        ("cs", ctypes.c_double),
        ("hp", ctypes.c_double),
        ("od", ctypes.c_double),
        ("bpm", ctypes.c_double),
        ("clockRate", ctypes.c_double),
        ("timePreempt", Optionf64),
        ("greatHitWindow", Optionf64),
        ("nCircles", Optionu32),
        ("nSliders", Optionu32),
        ("nSpinners", Optionu32),
        ("maxCombo", Optionu32),
    ]

    def __init__(self, mode: int = None, stars: float = None, pp: float = None, ppAcc: Optionf64 = None, ppAim: Optionf64 = None, ppFlashlight: Optionf64 = None, ppSpeed: Optionf64 = None, ppStrain: Optionf64 = None, nFruits: Optionu32 = None, nDroplets: Optionu32 = None, nTinyDroplets: Optionu32 = None, aimStrain: Optionf64 = None, speedStrain: Optionf64 = None, flashlightRating: Optionf64 = None, sliderFactor: Optionf64 = None, ar: float = None, cs: float = None, hp: float = None, od: float = None, bpm: float = None, clockRate: float = None, timePreempt: Optionf64 = None, greatHitWindow: Optionf64 = None, nCircles: Optionu32 = None, nSliders: Optionu32 = None, nSpinners: Optionu32 = None, maxCombo: Optionu32 = None):
        if mode is not None:
            self.mode = mode
        if stars is not None:
            self.stars = stars
        if pp is not None:
            self.pp = pp
        if ppAcc is not None:
            self.ppAcc = ppAcc
        if ppAim is not None:
            self.ppAim = ppAim
        if ppFlashlight is not None:
            self.ppFlashlight = ppFlashlight
        if ppSpeed is not None:
            self.ppSpeed = ppSpeed
        if ppStrain is not None:
            self.ppStrain = ppStrain
        if nFruits is not None:
            self.nFruits = nFruits
        if nDroplets is not None:
            self.nDroplets = nDroplets
        if nTinyDroplets is not None:
            self.nTinyDroplets = nTinyDroplets
        if aimStrain is not None:
            self.aimStrain = aimStrain
        if speedStrain is not None:
            self.speedStrain = speedStrain
        if flashlightRating is not None:
            self.flashlightRating = flashlightRating
        if sliderFactor is not None:
            self.sliderFactor = sliderFactor
        if ar is not None:
            self.ar = ar
        if cs is not None:
            self.cs = cs
        if hp is not None:
            self.hp = hp
        if od is not None:
            self.od = od
        if bpm is not None:
            self.bpm = bpm
        if clockRate is not None:
            self.clockRate = clockRate
        if timePreempt is not None:
            self.timePreempt = timePreempt
        if greatHitWindow is not None:
            self.greatHitWindow = greatHitWindow
        if nCircles is not None:
            self.nCircles = nCircles
        if nSliders is not None:
            self.nSliders = nSliders
        if nSpinners is not None:
            self.nSpinners = nSpinners
        if maxCombo is not None:
            self.maxCombo = maxCombo

    @property
    def mode(self) -> int:
        return ctypes.Structure.__get__(self, "mode")

    @mode.setter
    def mode(self, value: int):
        return ctypes.Structure.__set__(self, "mode", value)

    @property
    def stars(self) -> float:
        return ctypes.Structure.__get__(self, "stars")

    @stars.setter
    def stars(self, value: float):
        return ctypes.Structure.__set__(self, "stars", value)

    @property
    def pp(self) -> float:
        return ctypes.Structure.__get__(self, "pp")

    @pp.setter
    def pp(self, value: float):
        return ctypes.Structure.__set__(self, "pp", value)

    @property
    def ppAcc(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "ppAcc")

    @ppAcc.setter
    def ppAcc(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "ppAcc", value)

    @property
    def ppAim(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "ppAim")

    @ppAim.setter
    def ppAim(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "ppAim", value)

    @property
    def ppFlashlight(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "ppFlashlight")

    @ppFlashlight.setter
    def ppFlashlight(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "ppFlashlight", value)

    @property
    def ppSpeed(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "ppSpeed")

    @ppSpeed.setter
    def ppSpeed(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "ppSpeed", value)

    @property
    def ppStrain(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "ppStrain")

    @ppStrain.setter
    def ppStrain(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "ppStrain", value)

    @property
    def nFruits(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "nFruits")

    @nFruits.setter
    def nFruits(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "nFruits", value)

    @property
    def nDroplets(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "nDroplets")

    @nDroplets.setter
    def nDroplets(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "nDroplets", value)

    @property
    def nTinyDroplets(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "nTinyDroplets")

    @nTinyDroplets.setter
    def nTinyDroplets(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "nTinyDroplets", value)

    @property
    def aimStrain(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "aimStrain")

    @aimStrain.setter
    def aimStrain(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "aimStrain", value)

    @property
    def speedStrain(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "speedStrain")

    @speedStrain.setter
    def speedStrain(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "speedStrain", value)

    @property
    def flashlightRating(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "flashlightRating")

    @flashlightRating.setter
    def flashlightRating(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "flashlightRating", value)

    @property
    def sliderFactor(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "sliderFactor")

    @sliderFactor.setter
    def sliderFactor(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "sliderFactor", value)

    @property
    def ar(self) -> float:
        return ctypes.Structure.__get__(self, "ar")

    @ar.setter
    def ar(self, value: float):
        return ctypes.Structure.__set__(self, "ar", value)

    @property
    def cs(self) -> float:
        return ctypes.Structure.__get__(self, "cs")

    @cs.setter
    def cs(self, value: float):
        return ctypes.Structure.__set__(self, "cs", value)

    @property
    def hp(self) -> float:
        return ctypes.Structure.__get__(self, "hp")

    @hp.setter
    def hp(self, value: float):
        return ctypes.Structure.__set__(self, "hp", value)

    @property
    def od(self) -> float:
        return ctypes.Structure.__get__(self, "od")

    @od.setter
    def od(self, value: float):
        return ctypes.Structure.__set__(self, "od", value)

    @property
    def bpm(self) -> float:
        return ctypes.Structure.__get__(self, "bpm")

    @bpm.setter
    def bpm(self, value: float):
        return ctypes.Structure.__set__(self, "bpm", value)

    @property
    def clockRate(self) -> float:
        return ctypes.Structure.__get__(self, "clockRate")

    @clockRate.setter
    def clockRate(self, value: float):
        return ctypes.Structure.__set__(self, "clockRate", value)

    @property
    def timePreempt(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "timePreempt")

    @timePreempt.setter
    def timePreempt(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "timePreempt", value)

    @property
    def greatHitWindow(self) -> Optionf64:
        return ctypes.Structure.__get__(self, "greatHitWindow")

    @greatHitWindow.setter
    def greatHitWindow(self, value: Optionf64):
        return ctypes.Structure.__set__(self, "greatHitWindow", value)

    @property
    def nCircles(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "nCircles")

    @nCircles.setter
    def nCircles(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "nCircles", value)

    @property
    def nSliders(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "nSliders")

    @nSliders.setter
    def nSliders(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "nSliders", value)

    @property
    def nSpinners(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "nSpinners")

    @nSpinners.setter
    def nSpinners(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "nSpinners", value)

    @property
    def maxCombo(self) -> Optionu32:
        return ctypes.Structure.__get__(self, "maxCombo")

    @maxCombo.setter
    def maxCombo(self, value: Optionu32):
        return ctypes.Structure.__set__(self, "maxCombo", value)




class callbacks:
    """Helpers to define callbacks."""


class Calculator:
    __api_lock = object()

    def __init__(self, api_lock, ctx):
        assert(api_lock == Calculator.__api_lock), "You must create this with a static constructor." 
        self._ctx = ctx

    @property
    def _as_parameter_(self):
        return self._ctx

    @staticmethod
    def new(beatmap_path: str) -> Calculator:
        """"""
        ctx = ctypes.c_void_p()
        if not hasattr(beatmap_path, "__ctypes_from_outparam__"):
            beatmap_path = ctypes.cast(beatmap_path, ctypes.POINTER(ctypes.c_char))
        c_lib.calculator_new(ctx, beatmap_path)
        self = Calculator(Calculator.__api_lock, ctx)
        return self

    def __del__(self):
        c_lib.calculator_destroy(self._ctx, )
    def calculate(self, score_params) -> CalculateResult:
        """"""
        return c_lib.calculator_calculate(self._ctx, score_params)



class ScoreParams:
    __api_lock = object()

    def __init__(self, api_lock, ctx):
        assert(api_lock == ScoreParams.__api_lock), "You must create this with a static constructor." 
        self._ctx = ctx

    @property
    def _as_parameter_(self):
        return self._ctx

    @staticmethod
    def new() -> ScoreParams:
        """ 构造一个params"""
        ctx = ctypes.c_void_p()
        c_lib.score_params_new(ctx, )
        self = ScoreParams(ScoreParams.__api_lock, ctx)
        return self

    def __del__(self):
        c_lib.score_params_destroy(self._ctx, )
    def mode(self, mode: ctypes.c_int):
        """"""
        return c_lib.score_params_mode(self._ctx, mode)

    def mods(self, mods: int):
        """"""
        return c_lib.score_params_mods(self._ctx, mods)

    def acc(self, acc: float):
        """"""
        return c_lib.score_params_acc(self._ctx, acc)

    def n300(self, n300: int):
        """"""
        return c_lib.score_params_n300(self._ctx, n300)

    def n100(self, n100: int):
        """"""
        return c_lib.score_params_n100(self._ctx, n100)

    def n50(self, n50: int):
        """"""
        return c_lib.score_params_n50(self._ctx, n50)

    def combo(self, combo: int):
        """"""
        return c_lib.score_params_combo(self._ctx, combo)

    def score(self, score: int):
        """"""
        return c_lib.score_params_score(self._ctx, score)

    def n_misses(self, n_misses: int):
        """"""
        return c_lib.score_params_n_misses(self._ctx, n_misses)

    def n_katu(self, n_katu: int):
        """"""
        return c_lib.score_params_n_katu(self._ctx, n_katu)

    def passed_objects(self, passed_objects: int):
        """"""
        return c_lib.score_params_passed_objects(self._ctx, passed_objects)

    def clock_rate(self, clock_rate: float):
        """"""
        return c_lib.score_params_clock_rate(self._ctx, clock_rate)




use crate::*;
use interoptopus::{
    ffi_service, ffi_service_ctor, ffi_service_method, ffi_type, patterns::option::FFIOption,
};
use rosu_pp::{AnyPP, GameMode};

#[ffi_type(opaque)]
#[repr(C)]
#[derive(Clone, Default, PartialEq)]
#[allow(non_snake_case)]
pub struct ScoreParams {
    pub mode: FFIOption<Mode>,
    pub mods: u32,
    pub acc: FFIOption<f64>,
    pub n300: FFIOption<u32>,
    pub n100: FFIOption<u32>,
    pub n50: FFIOption<u32>,
    pub nMisses: FFIOption<u32>,
    pub nKatu: FFIOption<u32>,
    pub combo: FFIOption<u32>,
    pub passedObjects: FFIOption<u32>,
    pub clockRate: FFIOption<f64>,
}

#[ffi_service(error = "FFIError", prefix = "score_params_")]
impl ScoreParams {
    /// 构造一个params
    #[ffi_service_ctor]
    pub fn new() -> Result<Self, Error> {
        Ok(Self::default())
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn mode(&mut self, mode: Mode) {
        self.mode = Some(mode).into();
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn mods(&mut self, mods: u32) {
        self.mods = mods;
    }
    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn acc(&mut self, acc: f64) {
        self.acc = Some(acc).into();
    }
    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn n300(&mut self, n300: u32) {
        self.n300 = Some(n300).into();
    }
    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn n100(&mut self, n100: u32) {
        self.n100 = Some(n100).into();
    }
    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn n50(&mut self, n50: u32) {
        self.n50 = Some(n50).into();
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn combo(&mut self, combo: u32) {
        self.combo = Some(combo).into();
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn n_misses(&mut self, n_misses: u32) {
        self.nMisses = Some(n_misses).into();
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn n_katu(&mut self, n_katu: u32) {
        self.nKatu = Some(n_katu).into();
    }

    #[ffi_service_method(on_panic = "return_default")]
    pub fn passed_objects(&mut self, passed_objects: u32) {
        self.passedObjects = Some(passed_objects).into();
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn clock_rate(&mut self, clock_rate: f64) {
        self.clockRate = Some(clock_rate).into();
    }
}

impl ScoreParams {
    pub fn apply(self, mut calculator: AnyPP) -> AnyPP {
        let ScoreParams {
            mode,
            mods,
            n300,
            n100,
            n50,
            nMisses,
            nKatu,
            acc,
            combo,
            passedObjects,
            clockRate,
        } = self;

        if let Some(mode) = mode.into_option() {
            let mode = match mode {
                Mode::Osu => GameMode::Osu,
                Mode::Taiko => GameMode::Taiko,
                Mode::Catch => GameMode::Catch,
                Mode::Mania => GameMode::Mania,
            };

            calculator = calculator.mode(mode);
        }

        if let Some(n300) = n300.into_option() {
            calculator = calculator.n300(n300 as usize);
        }

        if let Some(n100) = n100.into_option() {
            calculator = calculator.n100(n100 as usize);
        }

        if let Some(n50) = n50.into_option() {
            calculator = calculator.n50(n50 as usize);
        }

        if let Some(n_misses) = nMisses.into_option() {
            calculator = calculator.n_misses(n_misses as usize);
        }

        if let Some(n_katu) = nKatu.into_option() {
            calculator = calculator.n_katu(n_katu as usize);
        }

        if let Some(combo) = combo.into_option() {
            calculator = calculator.combo(combo as usize);
        }

        if let Some(passed_objects) = passedObjects.into_option() {
            calculator = calculator.passed_objects(passed_objects as usize);
        }

        if let Some(clock_rate) = clockRate.into_option() {
            calculator = calculator.clock_rate(clock_rate);
        }

        calculator = calculator.mods(mods);

        if let Some(acc) = acc.into_option() {
            calculator = calculator.accuracy(acc);
        }

        calculator
    }
}

impl std::fmt::Debug for ScoreParams {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        use std::fmt::Display;
        write!(
            f,
            "ScoreParams {{ \
            mode: {}, \
            mods: {}, \
            n300: {}, \
            n100: {}, \
            n50: {}, \
            nMisses: {}, \
            nKatu: {}, \
            acc: {}, \
            combo: {}, \
            passedObjects: {}, \
            clockRate: {} \
        }}",
            match self.mode.into_option() {
                Some(ref mode) => mode as &dyn Display,
                None => &"None" as &dyn Display,
            },
            self.mods,
            match self.n300.into_option() {
                Some(ref n300) => n300 as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.n100.into_option() {
                Some(ref n100) => n100 as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.n50.into_option() {
                Some(ref n50) => n50 as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.nMisses.into_option() {
                Some(ref n_misses) => n_misses as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.nKatu.into_option() {
                Some(ref n_katu) => n_katu as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.acc.into_option() {
                Some(ref acc) => acc as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.combo.into_option() {
                Some(ref combo) => combo as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.passedObjects.into_option() {
                Some(ref passed_objects) => passed_objects as &dyn Display,
                None => &"None" as &dyn Display,
            },
            match self.clockRate.into_option() {
                Some(ref clock_rate) => clock_rate as &dyn Display,
                None => &"None" as &dyn Display,
            },
        )
    }
}

use crate::*;
use interoptopus::{
    ffi_service, ffi_service_ctor, ffi_service_method, ffi_type, patterns::string::AsciiPointer,
};
use rosu_pp::{AnyPP, Beatmap};

#[ffi_type(opaque)]
#[derive(Default)]
pub struct Calculator {
    pub inner: Beatmap,
}

// Regular implementation of methods.
#[ffi_service(error = "FFIError", prefix = "calculator_")]
impl Calculator {
    #[ffi_service_ctor]
    pub fn new(beatmap_path: AsciiPointer) -> Result<Self, Error> {
        let path = match beatmap_path.as_c_str() {
            Some(s) => s.to_str()?,
            None => return Err(Error::InvalidString(None)),
        };
        Ok(Self {
            inner: Beatmap::from_path(path)?,
        })
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn calculate(&mut self, score_params: ScoreParams) -> CalculateResult {
        let mods = score_params.mods;
        let clock_rate = score_params.clockRate;
        let calculator = score_params.apply(AnyPP::new(&self.inner));
        let result = CalculateResult::new(
            calculator.calculate(),
            &self.inner,
            mods,
            clock_rate.into_option(),
        );
        result
    }
}

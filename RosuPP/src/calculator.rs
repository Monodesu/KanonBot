use crate::*;
use interoptopus::{
    ffi_service, ffi_service_ctor, ffi_service_method, ffi_type,
    patterns::{option::FFIOption, slice::FFISlice},
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
    pub fn new(beatmap_data: FFISlice<u8>) -> Result<Self, Error> {
        Ok(Self {
            inner: Beatmap::from_bytes(beatmap_data.as_slice())?,
        })
    }

    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn calculate(&mut self, score_params: *const ScoreParams) -> CalculateResult {
        let score_params = unsafe {
            score_params.as_ref().unwrap_or_else(|| {
                panic!("！！未知的参数，score_params: {score_params:?}")
            })
        };
        let mods = score_params.mods;
        let clock_rate = score_params.clockRate;
        let calculator = score_params.apply(AnyPP::new(&self.inner));
        CalculateResult::new(
            calculator.calculate(),
            &self.inner,
            mods,
            clock_rate.into_option(),
        )
    }

    #[allow(non_snake_case)]
    #[ffi_service_method(on_panic = "undefined_behavior")]
    pub fn scorePos(&self, res: CalculateResult) -> FFIOption<f64> {
        let map = self.inner.convert_mode(res.mode.into());
        let idx = match res.mode {
            Mode::Osu | Mode::Taiko => {
                let (Some(nCircles), Some(nSliders), Some(nSpinners)) = (
                    res.nCircles.into_option(),
                    res.nSliders.into_option(),
                    res.nSpinners.into_option(),
                ) else { unreachable!() };
                nCircles + nSliders + nSpinners
            }
            Mode::Catch => {
                let (Some(nFruits), Some(nDroplets), Some(nTinyDroplets), Some(nSpinners)) = (
                    res.nFruits.into_option(),
                    res.nDroplets.into_option(),
                    res.nTinyDroplets.into_option(),
                    res.nSpinners.into_option(),
                ) else { unreachable!() };
                nFruits + nDroplets + nTinyDroplets + nSpinners
            }
            Mode::Mania => {
                let (Some(nCircles), Some(nSliders)) = (
                    res.nCircles.into_option(),
                    res.nSliders.into_option()
                ) else { unreachable!() };
                nCircles + nSliders
            }
        } as usize;
        map.hit_objects
            .iter()
            .enumerate()
            .find(|(i, _)| *i > idx)
            .map(|(_, o)| o.start_time)
            .into()
    }
}

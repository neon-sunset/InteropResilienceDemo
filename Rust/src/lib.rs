#![allow(non_snake_case)]

use std::sync::Barrier;

static mut BARRIER: Barrier = Barrier::new(1);

#[no_mangle]
pub extern "C" fn init(n: usize) {
    unsafe {
        BARRIER = Barrier::new(n);
    }
}

#[no_mangle]
pub extern "C" fn wait() {
    unsafe {
        BARRIER.wait();
    }
}

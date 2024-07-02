<script setup>
import { ref, watch, onMounted } from 'vue'
import { hocrm } from '../CRMHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    attName: {
    }
}
)

const value = ref(null)

watch(value, (newValue, oldValue) => {
    $emit('update:modelValue', newValue);
})

onMounted(() => {
    if (props.modelValue == null || props.modelValue == undefined) {
        value = null
    }
    else if (props.modelValue) {
        value = props.modelValue;
    }
})
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-input v-model="value" placeholder="Please input" type="number" />
        <el-icon size="2em" :color="required && (modelValue == null || modelValue == '') ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>
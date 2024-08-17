<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {

    },
    labelName: {
    },
    attributeName: {
    }
}
)

const value = ref(null)
const options = ref([
    {
        value: true,
        label: 'True',
    },
    {
        value: false,
        label: 'False',
    },
])
const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    if (newValue === '') {
        emit('update:modelValue', null);
    }
    else {
        emit('update:modelValue', newValue);
    }
})
watch(() => props.modelValue, (newValue, oldValue) => {
    value.value = newValue;
})

onMounted(() => {
    if (props.modelValue == null || props.modelValue == undefined) {
        value.value = null
    }
    else if (props.modelValue) {
        value.value = props.modelValue;
    }
})
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLabel">
            <p>{{ labelName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-select class="controlItem" v-model="value" placeholder="Select" clearable>
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
        </el-select>
        <el-icon class="requiredIcon" style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && modelValue == null" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>
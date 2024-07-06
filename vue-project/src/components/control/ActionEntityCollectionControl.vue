<script setup>
import { ref, watch, onMounted } from 'vue'

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

const value = ref("")
const isArray = ref(false)
const tip = ref(`
      <pre>
Please use standard JSON format and follow the specifications for the EntityCollection type input.
Here is an example:
[
  {
      "@odata.type": "Microsoft.Dynamics.CRM.account",
      "accountid": "C4CA0B66-59B9-E611-9999-C4346BDC0E01",
      "name": "hoho",
      "accountnumber": "123"
  },
  {
      "@odata.type": "Microsoft.Dynamics.CRM.account",
      "accountid": "CD67D78E-16BB-E611-9999-C4346BDC3C21",
      "name": "hoho2",
      "accountnumber": "456"
  }
]
</pre>
`)

const emit = defineEmits(['update:modelValue'])

watch(() => value.value, (newValue, oldValue) => {
    checkValue(newValue);
})

onMounted(() => {
    checkValue(value.value);
})

function checkValue(value) {
    let modelValue = value;
    if (!modelValue) {
        modelValue = null;
    }
    try {
        let Jvalue = JSON.parse(value);
        if (typeof Jvalue == 'object') {
            modelValue = Jvalue;
            isArray.value = Array.isArray(Jvalue);
        }
        else {
            isArray.value = false;
        }
    } catch (error) {
        isArray.value = false;
    }
    emit('update:modelValue', modelValue);
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-input v-model="value" placeholder="Please input" type="textarea" />
        <el-tooltip placement="top" effect="light" :content="tip" raw-content>
            <el-icon>
                <Warning />
            </el-icon>
        </el-tooltip>
        <el-icon size="2em" :color="(!required && !value) || isArray ? '#67C23A' : '#F56C6C'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>